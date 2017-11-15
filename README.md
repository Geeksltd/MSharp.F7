## SmartF7: Instant File Switcher

This is a Visual Studio extension that can improve file navigation for certain project types. When you press F7 it jumps to the sister file of the current file. When there are multiple sister files, repeating F7 will keep rotating between the sisters.

## M# Entity type
F7 will rotate between: 
- @Model\Entities\Customer.cs
- Domain\Entities\Customer.cs
- Domain\Logic\Customer.cs


## M# MVC module (Form, list, ...)

### With a single host page
F7 will rotate between:  
- @UI\Modules\Customer\CustomersList.cs
- Website\Controllers\Pages\TheHostPage.Controller.cs 
- Website\Views\Pages\TheHostPage.cshtml 

### With multiple host pages (shared module)
F7 will rotate between: 
- @UI\Modules\Customer\CustomersList.cs
- Website\Controllers\Modules\CustomersListController.cs 
- Website\Views\Modules\CustomersList.cshtml

### View component
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
