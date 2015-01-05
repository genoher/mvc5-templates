Changes to NorthWnd EF Model
============================

After running the EF-Code First wizard the following changes were made.
1. Added Name property, in addtional partial classes, to Employee, Shipper, Customer
2. Added Foreign Key column attributes to Order

After Running the MVC 5 Controller with views, using Entity Framework wizard
============================================================================
1. Created the OrderViewModel class by 'Ctrl + .'
2. Made the OrderViewModel public
3. Made the Customers, Orders, Shippers and Order properties with 'Ctrl + .'
4. Corrected OrderBy functions in OrdersController.RefreshViewModel
5. Corrected the Shippers DropDownList statement in the Create and Edit views. Northwind uses a
funnny named key here where Orders.ShipVia maps to Shippers.ShipperID