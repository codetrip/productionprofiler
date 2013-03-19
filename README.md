ProductionProfiler is an unobtrusive extension to any ASP.NET Web Forms/MVC application, written in C#, which allows you to capture detailed debugging information whilst your application is running in a production environment.

There are three ways you can configure the profiler to run, either by URL, Session or via sampling.

The admin interface allows you to configure the profiler, for URL based profiling you can register a URL (regular expressions are supported) within your application to be profiled, optionally specifying the number of times you want them profiled and the server on which to profile.

Session based profiling works by initializing a session, with a unique Id, this writes a cookie to your browser and all subsequent requests will be profiled.

Finally sampling works by specifying a sample frequency and time period, the profiler will then run for the specified period each tine the frequency time period elapses.

The profiler works by intercepting any component you tell it to profile when they are resolved by your IoC container. All method calls to the resolved components are then proxied allowing the profiler to capture information such as input / output data, method execution time etc. The full response of each request is also captured allowing you to see this in the context of your application (i.e. all CSS & JS is rendered with the response)

The profiler can capture all information logged via log4net, which is captured against the method being executed.

There are multiple extension points to allow you to write custom data collectors for particular types.

The profiler comes out of the box with support for the following application technologies

SQL Server, SQLite, MongoDB and RavenDB for persistence

Castle Windsor and StructureMap IoC containers.

Log4Net for capturing logging/trace information.

