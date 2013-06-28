Spinnaker
=========
Cross-platform HTML GUI for .NET/Mono apps.

Introduction
------------
Spinnaker allows you to use HTML as the GUI for your desktop and mobile applications. It is a lightweight, cross-platform framework that works with .NET and Mono.

What Spinnaker Does For You
---------------------------
+ Spinnaker allows you to bind elements in the HTML GUI to simple C# classes in your application. When properties on your classes change, the GUI is updated to match. When the user interacts with the HTML GUI, methods on your C# classes are invoked. This is [the MVVM pattern](http://en.wikipedia.org/wiki/Model_View_ViewModel), and these classes are your View Models.
+ Handles the plumbing of loading and rendering HTML views on each platform. You simply write ViewModels and bundle HTML into your project.
+ Use HTML for all of your application's GUI or mix it with other native UI elements. Spinnaker doesn't care whether it is running alone or intermingled with other kinds of GUI.
+ Allows you to use the same GUI, without code changes or markup changes, on many desktop and mobile platforms.
+ Uses [Knockout.js](http://knockoutjs.com/) as the binding engine in the HTML. You don't have to learn some custom-built, immature, niche binding syntax for use only with this tool. 

What Spinnaker Does Not Do
--------------------------
+ Spinnaker does not provide out of the box HTML GUI. You write your own HTML, using any tools or libraries that you like. For example, you're welcome to use something like [Bootstrap](http://twitter.github.com/bootstrap/), [Wijmo](http://wijmo.com/), other toolkits, or just raw markup and CSS of your own to create a nice GUI with. Spinnaker doesn't care, nor does it try to help.
+ You're stuck with Knockout.js for data-binding. If someone is interested in a different binding engine, file an issue. The dependency on Knockout.js is actually fairly thin.

Why?
----
Many of us are still writing "installed applications" - apps that live on the user's desktop computer or device instead of on a web server. Reasons for this include such requirements as:
+ Use of USB/HID devices on the user's computer
+ Unfettered access to the native file system
+ Use of platform-specific API's or services
+ High performance 3D/Audio applications that can't tolerate the "almost there" state of WebGL and high latency audio of browsers

This is a small sampling of the many reasons you might need to deliver something other than a web application to your users. But as publishers of installed software, it is desirable for us to be able to use all of the richness of modern web interfaces and to take advantage of the abudance of design talent and tooling that serves the HTML market. It is also highly desirable to write our GUI once for all of our target platforms rather than writing a new, native GUI on each of our target platforms.

Spinnaker allows you to enjoy the benefits of HTML design and tooling whether you are targeting a single platform or multiple.

Why Not?
--------
Some reasons that you might not want to use Spinnaker:
+ For a web application. If your application is delivered in the browser, then this does nothing for you.
+ You're targeting an unsupported platform (see below).
+ You don't like the MVVM pattern. (What's wrong with you, anyway?)

Supported Platforms
------
Spinnaker already works here:
+ Windows XP/Vista/7/8 in Windows Forms applications
+ Windows XP/Vista/7/8 in WPF applications
+ Mac OS X 10.6+ in MonoMac applications
+ iOS 6+ in MonoTouch applications
+ Linux with GTK# and webkit-sharp

Support is also planned, but not yet available, for these platforms:
+ Android

Support isn't planned for anything else at this time, so if you need another platform then feel free to contribute.

Getting Started
----
There are several .sln files in the repository. You can use them in Visual Studio 2012 or MonoDevelop. You can either open the solution for the particular platform that you want to work with first (such as SpinnakeMac.sln to work on Mac OS X, or SpinnakerWindows.sln to work with WPF and WinForms on Windows, etc.), or you can open SpinnakerAllPlatforms.sln to work with all of them at once. Note that you can only build WPF on Windows with Visual Studio and you can only build Mac with MonoMac on a Mac. So either way, when using the AllPlatforms solution, you will only be able to build the subset that is possible on one build machine. You will also receive notifications from Visual Studio or MonoDevelop that some projects aren't supported.

All of the solutions include a sample application for each platform. The sample demonstrates most of Spinnaker's features.
 
