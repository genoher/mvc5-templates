# MVC5 Templates
MVC5 Templates is a revised version of the *MVC5 Controller with views, using Entity Framework* T4 templates that ship with MVC5. MVC5 Templates are slightly more opinionated, make a couple more assumptions and hence, are able to do a bit more for you.

## What's been changed?
#### Controllers
* DB persistence/changes wrap a fresh DbContext object in a using statement
    using(var db = new DbContext()){... db.SaveChanges();}
* The module level DbContext object creates simple POCOs that are not proxied or tracked. It's just used for quick data retrieval.
* ViewModel!! Any related foreign key relationship data required to populate drop down lists in the view are passed as a `List<T>` into a strongly typed ViewModel.

#### Views
* Bootstrap's `form-group` and `form-control` CSS classes used throughout. (This has meant a requirement on System.Web.Mvc 5.1.0.0 or above).
* `DropDownList<TModel>` helper methods are completed in full and refer to the ViewModel defined in the Controller class.
* The `viewbag.title` property is set once and re-used
* *Back To List* links have been changed into Cancel buttons.
* The `Index` view uses a responsive, striped, bordered, condensed table.
* The `Delete` view asks a more specific question.

Basically, all the things that annoy me or I find I'm fixing every time I run the MVC Controller wizard have been fixed.

## About this solution
The only true deliverables for this solution are the contents of the `CodeTemplates` folder. There's a post successful build task that copies the contents of the CodeTemplates folder up to the root. Those are the files that should be included in a CodeTemplates folder in your own MVC project. Or... just import the <a href="https://www.nuget.org/packages/MVC5Templates/">Nuget</a> package instead.

The Northwind DB model is used to demo and test the T4 templates. Entity Framework Code First objects have been reverse-engineered from the familiar SQL Server Northwind DB. The project has then been configured to create a new localdb with some initialisation code to populate a local mdf file with Northwind data for Shippers, Customers, Employees and Orders. So the solution should work out of the box, no SQL Server needs to be installed and when you press F5 you should get a new freshly populated DB every time.

The templates make a couple of convention assumptions that the Northwnd DB doesn't follow. Firstly that entities have a `Name` property and secondly that foreign keys have the same name in both tables. In order to preserve the Northwind model, and allow it be re-generated from scratch if needs be, additional partial classes have been created for `Customer`, `Employee` and `Shipper` that map the assumed missing properties. The Name property is used in building out the drop down lists and in your own project you could just replace Name with something appropriate for your Model or get with the program just use Name :-D.

## Using the templates in your own project
1. Copy the CodeTemplates folder into your MVC project and include all the files as part of the project (or just import the <a href="https://www.nuget.org/packages/MVC5Templates/">Nuget</a> package :-D).
2. Run the MVC controller wizard to build the controller and views that you require. You'll need to select the *MVC5 Controller with views, using Entity Framework* option.
3. *Ctrl + .* any new ViewModels into existence. The scaffolding templates can't actually create the ViewModel class for you in your project so......the easiest way is to get Visual Studio to create what's missing (Ctrl+. brings up the context helper menu where you can select *Generate class for 'FooViewModel'*). Once you've Ctrl+.'ed the ViewModel into existence you should add a `public` access modifier to the class and in the controller scroll down to the `RefreshViewModel` private function. You'll need to Ctrl+. -> Generate Property Stub each of the `List<T>` collections set on the ViewModel. Finally, the ViewModel class will also have a property for the actual Model object itself that needs to be generated. Scroll to the `Create [HttpPost]` or either of the `Edit` methods and Ctrl+. that into existence as well. Basically, when your project builds you're done with the Ctrl+.
