## SmartF7: Instant File Switcher

This is a Visual Studio extension that can improve file navigation for certain project types. Often when you're working with code, for every concept there are multiple code files, which are effectively sister files all related to that concept.

With this extension, pressing F7 will quickly jump switch to the sister files of the current file in Visual Studio. When there are multiple sister files, repeating F7 will keep rotating between the sisters.

## M# Entity type
F7 will rotate between: 
- @Model\{namespace}\Customer.cs
- Domain\Entities\Customer.cs
- Domain\-Logic\Customer.cs

## M# MVC - page
F7 will rotate between:  
- @UI\Pages\P1\P2\SomePage.cs
- Website\Controllers\Pages\P1-P2--SomePage.Controller.cs 
- Website\Views\Pages\P1P2SomePage.cshtml 


## M# MVC - module (Form, list, ...)

#### With a single host page
F7 will rotate between:  
- @UI\Modules\Customer\CustomersList.cs
- Website\Controllers\Pages\TheHostPage.Controller.cs 
- Website\Views\Pages\TheHostPage.cshtml 

>Note: When an M# module is hosted on a single page only, it's effectively owned by that module. Therefore the module related code will be generated inside the page controller and view.

#### With multiple host pages (shared module)
F7 will rotate between: 
- @UI\Modules\Customer\CustomersList.cs
- Website\Controllers\Modules\CustomersListController.cs 
- Website\Views\Modules\CustomersList.cshtml

#### View component
F7 will rotate between: 
- @UI\Modules\Customer\CustomersList.cs
- Website\Controllers\Modules\Components\CustomersList.cs 
- Website\Views\Modules\Components\CustomersList\Default.cshtml



## M# Web Forms (aspx & ascx)
F7 will rotate between: 
- Website\Modules\ViewCustomer.ascx
- Website\Modules\ViewCustomer.ascx.cs


## Zebble for Xamarin
F7 will rotate between: 
-App.UI\Views\Modules\Customer.zbl
-App.UI\Views\Modules\Customer.zbl.cs
