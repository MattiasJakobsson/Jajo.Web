require 'bundler/setup'

require 'albacore'
require 'albacore/tasks/versionizer'
require 'fileutils'

Configuration = ENV['CONFIGURATION'] || 'Debug'

Albacore::Tasks::Versionizer.new :versioning

desc 'create assembly infos'
asmver :asmver do |a|
  a.file_path  = 'src/CommonAssemblyInfo.cs'
  a.attributes assembly_version: ENV['LONG_VERSION'],
    assembly_file_version: ENV['LONG_VERSION']
  a.using 'System'
end

desc 'Perform fast build (warn: doesn\'t d/l deps)'
build :quick_compile do |b|
  b.prop 'Configuration', Configuration
  b.logging = 'detailed'
  b.sln     = 'src/SuperGlue.sln'
end

task :tests do |b|
  FileUtils.mkdir_p 'build/tests'
  FileUtils.rm_rf(Dir.glob('build/tests/*'))
  testIndex = 0
  FileList['**/*.Tests/bin/' + Configuration + '/*.Tests.dll'].each do |test|
	resultFile = 'build/tests/TestResult' + testIndex.to_s + '.xml'
    system 'packages/Fixie/lib/net45/Fixie.Console.x86.exe', test, '--xUnitXml', resultFile
	testIndex = testIndex + 1
  end
end

task :paket_bootstrap do
  system 'tools/paket.bootstrapper.exe', clr_command: true unless File.exists? 'tools/paket.exe'
end

desc 'restore all nugets as per the packages.config files'
task :restore => :paket_bootstrap do
  system 'tools/paket.exe', 'restore', clr_command: true
end

desc 'Perform full build'
build :compile => [:versioning, :restore, :asmver] do |b|
  b.prop 'Configuration', Configuration
  b.sln = 'src/SuperGlue.sln'
end

directory 'build/pkg'

desc 'package nugets - finds all projects and package them'
task :create_nugets => [:versioning] do |p|
  FileUtils.rm_rf(Dir.glob('build/pkg/*'))
  system 'tools/paket.exe', 'pack', 'output', 'build/pkg', 'buildconfig', Configuration, 'version', ENV['NUGET_VERSION']
end

task :default => :compile

task :ci => [:default, :tests, :create_nugets]

task :test => [:default, :tests]