# library-app-3
C# course - ex3
_________

### To run program:  
1. Connect to SQL Server  
2. create new database 'library_db_test'  
3. copy program.cs to visual studio project (make sure namespace at the top of the file matches solution name)  
4. add SQL package  

  In terminal:  
  - dotnet add package System.Data.SqlClient  
  OR  
  - Right Click on your project file in the solution panel, and then Click on the Manage NuGet Packages option.  
  In the NuGet Package Manager window, Select the Browser Tab. Search for System.Data.SqlClient and Press enter  
  Select the first option, System.Data.SqlClient by Microsoft Click on the install button
5. run the program  


_____

Note: On the first run the program will create tables and initialize them with some data.
      After tables get created it will not call those functions again on the next run.
