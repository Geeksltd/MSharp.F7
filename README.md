# VSIX.SmartF7: Instant File Switcher

This is a Visual Studio extension that can improve file navigation for certain project types.

> Press F7 to jump to the sister file of the current file quickly.

## All M# project types
- From a generated generated entity file jumps to its Logic partial file (and vice versa).
> For example from Domain\Entities\Customer.cs   🡲  Domain\-Logic\Customer.cs

## M# Web Forms
- From an ASCX file, jumps to its code behind (and vice versa).
> For example from Website\Modules\ViewCustomer.ascx   🡲   Website\Modules\ViewCustomer.ascx.cs

## M# MVC
- From a controller file, jumps to the View (cshtml) file (and vice versa).
> For example from Website\Controllers\ViewCustomerControllers.cs   🡲   Website\Views\ViewCustomer.cshtml

## M# .NET Core
- From an MC# file (meta definition) it will jump to the generated file (entity, module or page).
> For example from @MSharp.Domain\Entities\Customer.cs   🡲   Domain\Entities\Customer.cs

## Zebble for Xamarin
It will switch between a .ZBL markup file and its C# codebehind.
> For example from App.UI\Views\Modules\Customer.zbl   🡲   App.UI\Views\Modules\Customer.zbl.cs
