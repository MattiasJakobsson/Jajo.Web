using System.Collections.Generic;
using System.IO;
using Should;
using SuperGlue.Web.ModelBinding.BindingSources;

namespace SuperGlue.Web.ModelBinding.Tests
{
    public class JsonRequestBodyBindingSourceTests
    {
        public void when_parsing_simple_object_result_should_contain_correct_data()
        {
            var result = ParseJson("{'Name':'Mattias'}");

            result.Count.ShouldEqual(1);
            result.ContainsKey("name").ShouldBeTrue();
            result["name"].ShouldEqual("Mattias");
        }

        public void when_parsing_complex_one_level_object_result_should_contain_correct_data()
        {
            var result = ParseJson("{'Result': {'FirstName':'Mattias', 'LastName':'Jakobsson'}}");

            result.Count.ShouldEqual(2);
            result.ContainsKey("result_firstname").ShouldBeTrue();
            result.ContainsKey("result_lastname").ShouldBeTrue();
            result["result_firstname"].ShouldEqual("Mattias");
            result["result_lastname"].ShouldEqual("Jakobsson");
        }

        public void when_parsing_object_with_simple_collection_at_first_level_result_should_contain_correct_data()
        {
            var result = ParseJson("{'Results':['Mattias', 'Jakobsson']}");

            result.Count.ShouldEqual(2);
            result.ContainsKey("results[0]_").ShouldBeTrue();
            result.ContainsKey("results[1]_").ShouldBeTrue();
            result["results[0]_"].ShouldEqual("Mattias");
            result["results[1]_"].ShouldEqual("Jakobsson");
        }

        public void when_parsing_object_with_complex_collection_at_first_level_result_should_contain_correct_data()
        {
            var result = ParseJson("{'Results':[{'Name': 'Mattias'}, {'Name':'Jakobsson'}]}");

            result.Count.ShouldEqual(2);
            result.ContainsKey("results[0]_name").ShouldBeTrue();
            result.ContainsKey("results[1]_name").ShouldBeTrue();
            result["results[0]_name"].ShouldEqual("Mattias");
            result["results[1]_name"].ShouldEqual("Jakobsson");
        }

        public void when_parsing_object_with_collection_in_collection_result_should_contain_correct_data( )
        {
            var result = ParseJson("{'Results':[[{'Name':'Mattias'}], [{'Name': 'Jakobsson'}]]}");

            result.Count.ShouldEqual(2);
            result.ContainsKey("results[0][0]_name").ShouldBeTrue();
            result.ContainsKey("results[1][0]_name").ShouldBeTrue();
            result["results[0][0]_name"].ShouldEqual("Mattias");
            result["results[1][0]_name"].ShouldEqual("Jakobsson");
        }

        private static IDictionary<string, object> ParseJson(string json)
        {
            var body = new MemoryStream();
            var writer = new StreamWriter(body);

            writer.Write(json);
            writer.Flush();

            body.Position = 0;

            var environment = new Dictionary<string, object>
            {
                {"owin.RequestBody", body}
            };

            var parser = new JsonRequestBodyBindingSource();

            return parser.GetValues(environment).Result;
        }
    }
}