## SmartF7: Instant File Switcher

This is a Visual Studio extension that can improve file navigation for certain project types. When you press F7 it jumps to the sister file of the current file.

## M# Entity type
It will rotate between related between different files of an Entity type: 
- @Model\Entities\Customer.cs
- Domain\Entities\Customer.cs
- Domain\Logic\Customer.cs


## M# MVC module (Form, list, ...)

### With a single host page
It will rotate between related between different files of a module: 
- @UI\Modules\Customer\CustomersList.cs
- Website\Controllers\Pages\TheHostPage.Controller.cs 
- Website\Views\Pages\TheHostPage.cshtml 

### With multiple host pages (shared module)
It will rotate between related between different files of a module: 
- @UI\Modules\Customer\CustomersList.cs
- Website\Controllers\Modules\CustomersListController.cs 
- Website\Views\Modules\CustomersList.cshtml

### View component
If a module is set to be a ViewComponent, then it will rotate between the following files: 
- @UI\Modules\Customer\CustomersList.cs
- Website\Controllers\Modules\Components\CustomersList.cs 
- Website\Views\Modules\Components\CustomersList\Default.cshtml



## M# Web Forms
- From an ASCX file, jumps to its code behind (and vice versa).
> For example from Website\Modules\ViewCustomer.ascx   🡲   Website\Modules\ViewCustomer.ascx.cs


## Zebble for Xamarin
It will switch between a .ZBL markup file and its C# codebehind.
> For example from App.UI\Views\Modules\Customer.zbl   🡲   App.UI\Views\Modules\Customer.zbl.cs
