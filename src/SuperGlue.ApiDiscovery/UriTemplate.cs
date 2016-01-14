﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperGlue.ApiDiscovery
{
    /// <summary>
    /// this is https://github.com/tavis-software/UriTemplates
    /// an RFC6570-compliant level-4 UriTemplate handler
    /// </summary>
    public class UriTemplate
    {
        const string UriReservedSymbols = ":/?#[]@!$&'()*+,;=";
        const string UriUnreservedSymbols = "-._~";

        static readonly Dictionary<char, OperatorInfo> Operators = new Dictionary<char, OperatorInfo>() {
            {'\0', new OperatorInfo {Default = true, First = "", Seperator = ',', Named = false, IfEmpty = "",AllowReserved = false}},
            {'+', new OperatorInfo {Default = false, First = "", Seperator = ',', Named = false, IfEmpty = "",AllowReserved = true}},
            {'.', new OperatorInfo {Default = false, First = ".", Seperator = '.', Named = false, IfEmpty = "",AllowReserved = false}},
            {'/', new OperatorInfo {Default = false, First = "/", Seperator = '/', Named = false, IfEmpty = "",AllowReserved = false}},
            {';', new OperatorInfo {Default = false, First = ";", Seperator = ';', Named = true, IfEmpty = "",AllowReserved = false}},
            {'?', new OperatorInfo {Default = false, First = "?", Seperator = '&', Named = true, IfEmpty = "=",AllowReserved = false}},
            {'&', new OperatorInfo {Default = false, First = "&", Seperator = '&', Named = true, IfEmpty = "=",AllowReserved = false}},
            {'#', new OperatorInfo {Default = false, First = "#", Seperator = ',', Named = false, IfEmpty = "",AllowReserved = true}}
        };

        readonly string _template;
        readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        enum States
        {
            CopyingLiterals,
            ParsingExpression
        }

        bool _errorDetected;
        StringBuilder _result;
        List<string> _parameterNames;

        public UriTemplate(string template)
        {
            _template = template;
        }


        public void SetParameter(string name, object value)
        {
            _parameters[name] = value;
        }

        public void SetParameter(string name, string value)
        {
            _parameters[name] = value;
        }

        public void SetParameter(string name, IEnumerable<string> value)
        {
            _parameters[name] = value;
        }

        public void SetParameter(string name, IDictionary<string, string> value)
        {
            _parameters[name] = value;
        }


        public IEnumerable<string> GetParameterNames()
        {
            var parameterNames = new List<string>();
            _parameterNames = parameterNames;
            Resolve();
            _parameterNames = null;
            return parameterNames;
        }

        public string Resolve()
        {
            var currentState = States.CopyingLiterals;
            _result = new StringBuilder();
            StringBuilder currentExpression = null;
            foreach (var character in _template.ToCharArray())
            {
                switch (currentState)
                {
                    case States.CopyingLiterals:
                        if (character == '{')
                        {
                            currentState = States.ParsingExpression;
                            currentExpression = new StringBuilder();
                        }
                        else if (character == '}')
                        {
                            throw new ArgumentException("Malformed template, unexpected } : " + _result);
                        }
                        else
                        {
                            _result.Append(character);
                        }
                        break;
                    case States.ParsingExpression:
                        if (character == '}')
                        {
                            ProcessExpression(currentExpression);

                            currentState = States.CopyingLiterals;
                        }
                        else
                        {
                            currentExpression?.Append(character);
                        }

                        break;
                }
            }
            if (currentState == States.ParsingExpression)
            {
                _result.Append("{");
                if (currentExpression != null) _result.Append(currentExpression);

                throw new ArgumentException("Malformed template, missing } : " + _result);
            }

            if (_errorDetected)
            {
                throw new ArgumentException("Malformed template : " + _result);
            }
            return _result.ToString();
        }

        void ProcessExpression(StringBuilder currentExpression)
        {

            if (currentExpression.Length == 0)
            {
                _errorDetected = true;
                _result.Append("{}");
                return;
            }

            OperatorInfo op = GetOperator(currentExpression[0]);

            var firstChar = op.Default ? 0 : 1;


            var varSpec = new VarSpec(op);
            for (int i = firstChar; i < currentExpression.Length; i++)
            {
                char currentChar = currentExpression[i];
                switch (currentChar)
                {
                    case '*':
                        varSpec.Explode = true;
                        break;
                    case ':': // Parse Prefix Modifier
                        var prefixText = new StringBuilder();
                        currentChar = currentExpression[++i];
                        while (currentChar >= '0' && currentChar <= '9' && i < currentExpression.Length)
                        {
                            prefixText.Append(currentChar);
                            i++;
                            if (i < currentExpression.Length) currentChar = currentExpression[i];
                        }
                        varSpec.PrefixLength = int.Parse(prefixText.ToString());
                        i--;
                        break;
                    case ',':
                        var success = ProcessVariable(varSpec);
                        bool isFirst = varSpec.First;
                        // Reset for new variable
                        varSpec = new VarSpec(op);
                        if (success || !isFirst) varSpec.First = false;

                        break;


                    default:
                        if (IsVarNameChar(currentChar))
                        {
                            varSpec.VarName.Append(currentChar);
                        }
                        else
                        {
                            _errorDetected = true;
                        }
                        break;
                }
            }
            ProcessVariable(varSpec);

        }

        bool ProcessVariable(VarSpec varSpec)
        {
            var varname = varSpec.VarName.ToString();
            _parameterNames?.Add(varname);

            if (!_parameters.ContainsKey(varname)
                || _parameters[varname] == null
                || (_parameters[varname] is IList && ((IList)_parameters[varname]).Count == 0)
                || (_parameters[varname] is IDictionary && ((IDictionary)_parameters[varname]).Count == 0))
                return false;

            if (varSpec.First)
            {
                _result.Append(varSpec.OperatorInfo.First);
            }
            else
            {
                _result.Append(varSpec.OperatorInfo.Seperator);
            }

            object value = _parameters[varname];

            // Handle Strings
            var s = value as string;
            if (s != null)
            {
                var stringValue = s;
                if (varSpec.OperatorInfo.Named)
                {
                    AppendName(varname, varSpec.OperatorInfo, string.IsNullOrEmpty(stringValue));
                }
                AppendValue(stringValue, varSpec.PrefixLength, varSpec.OperatorInfo.AllowReserved);
            }
            else
            {
                // Handle Lists
                var list = value as IEnumerable<string>;
                if (list != null)
                {
                    var enumerable = list as string[] ?? list.ToArray();
                    if (varSpec.OperatorInfo.Named && !varSpec.Explode) // exploding will prefix with list name
                    {
                        AppendName(varname, varSpec.OperatorInfo, !enumerable.Any());
                    }

                    AppendList(varSpec.OperatorInfo, varSpec.Explode, varname, enumerable);
                }
                else
                {

                    // Handle associative arrays
                    var dictionary = value as IDictionary<string, string>;
                    if (dictionary != null)
                    {
                        if (varSpec.OperatorInfo.Named && !varSpec.Explode) // exploding will prefix with list name
                        {
                            AppendName(varname, varSpec.OperatorInfo, !dictionary.Any());
                        }
                        AppendDictionary(varSpec.OperatorInfo, varSpec.Explode, dictionary);
                    }

                }

            }
            return true;
        }


        void AppendDictionary(OperatorInfo op, bool explode, IDictionary<string, string> dictionary)
        {
            foreach (string key in dictionary.Keys)
            {
                _result.Append(key);
                _result.Append(explode ? '=' : ',');
                AppendValue(dictionary[key], 0, op.AllowReserved);

                _result.Append(explode ? op.Seperator : ',');
            }
            if (dictionary.Any())
            {
                _result.Remove(_result.Length - 1, 1);
            }
        }

        void AppendList(OperatorInfo op, bool explode, string variable, IEnumerable<string> list)
        {
            var enumerable = list as string[] ?? list.ToArray();
            foreach (string item in enumerable)
            {
                if (op.Named && explode)
                {
                    _result.Append(variable);
                    _result.Append("=");
                }
                AppendValue(item, 0, op.AllowReserved);

                _result.Append(explode ? op.Seperator : ',');
            }
            if (enumerable.Any())
            {
                _result.Remove(_result.Length - 1, 1);
            }
        }

        void AppendValue(string value, int prefixLength, bool allowReserved)
        {

            if (prefixLength != 0)
            {
                if (prefixLength < value.Length)
                {
                    value = value.Substring(0, prefixLength);
                }
            }

            _result.Append(Encode(value, allowReserved));

        }

        void AppendName(string variable, OperatorInfo op, bool valueIsEmpty)
        {
            _result.Append(variable);
            _result.Append(valueIsEmpty ? op.IfEmpty : "=");
        }


        bool IsVarNameChar(char c)
        {
            return ((c >= 'A' && c <= 'z') //Alpha
                    || (c >= '0' && c <= '9') // Digit
                    || c == '_'
                    || c == '%'
                    || c == '.');
        }

        static string Encode(string p, bool allowReserved)
        {

            var result = new StringBuilder();
            foreach (char c in p)
            {
                if ((c >= 'A' && c <= 'z') //Alpha
                    || (c >= '0' && c <= '9') // Digit
                    || UriUnreservedSymbols.IndexOf(c) != -1
                    // Unreserved symbols  - These should never be percent encoded
                    || (allowReserved && UriReservedSymbols.IndexOf(c) != -1))
                // Reserved symbols - should be included if requested (+)
                {
                    result.Append(c);
                }
                else
                {
#if PCL
                         result.Append(HexEscape(c));  
#else
                    var s = c.ToString();

                    var chars = s.Normalize(NormalizationForm.FormC).ToCharArray();
                    foreach (var ch in chars)
                    {
                        result.Append(HexEscape(ch));
                    }
#endif

                }
            }

            return result.ToString();


        }

        public static string HexEscape(char c)
        {
            var esc = new char[3];
            esc[0] = '%';
            esc[1] = HexDigits[((c & 240) >> 4)];
            esc[2] = HexDigits[(c & 15)];
            return new string(esc);
        }

        static readonly char[] HexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        static OperatorInfo GetOperator(char operatorIndicator)
        {
            OperatorInfo op;
            switch (operatorIndicator)
            {

                case '+':
                case ';':
                case '/':
                case '#':
                case '&':
                case '?':
                case '.':
                    op = Operators[operatorIndicator];
                    break;

                default:
                    op = Operators['\0'];
                    break;
            }
            return op;
        }


        public class OperatorInfo
        {
            public bool Default { get; set; }
            public string First { get; set; }
            public char Seperator { get; set; }
            public bool Named { get; set; }
            public string IfEmpty { get; set; }
            public bool AllowReserved { get; set; }

        }

        public class VarSpec
        {
            public StringBuilder VarName = new StringBuilder();
            public bool Explode;
            public int PrefixLength;
            public bool First = true;

            public VarSpec(OperatorInfo operatorInfo)
            {
                OperatorInfo = operatorInfo;
            }

            public OperatorInfo OperatorInfo { get; }
        }
    }
}