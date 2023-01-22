CREATE DATABASE EmployeeRecords
GO
	USE EmployeeRecords
GO
	CREATE TABLE Departments (
		Id int IDENTITY(1, 1) PRIMARY KEY,
		Name varchar(100) NOT NULL UNIQUE
	)
GO
	CREATE TABLE Employees (
		Id int IDENTITY(1, 1) PRIMARY KEY,
		Name varchar(100) NOT NULL,
		DateOfBirth DATE NOT NULL CHECK (DateOfBirth >= DATEADD(year, -18, GETDATE())),
		Address VARCHAR(200) NOT NULL,
		DepartmentId INT NOT NULL FOREIGN KEY REFERENCES Departments(Id) ON DELETE CASCADE
	)
	
CREATE INDEX IX_Employees_Name ON Employees (Name);
CREATE INDEX IX_Employees_DepartmentId ON Employees (DepartmentId);

GO
	CREATE TABLE EmployeesFiles(
		Id uniqueidentifier PRIMARY KEY default NEWID(),
		Name varchar(100) NOT NULL,
		Size float NOT NULL,
		Path varchar(200) NOT NULL,
		EmployeeId int NOT NULL FOREIGN KEY REFERENCES Employees(Id) ON DELETE CASCADE
	)
	
CREATE INDEX IX_EmployeesFiles_Name ON EmployeesFiles (Name);
CREATE INDEX IX_EmployeesFiles_EmployeeId ON EmployeesFiles (EmployeeId);

DECLARE @CheckBirthDateConstraintName varchar(255)
SELECT @CheckBirthDateConstraintName = c.name
FROM sys.check_constraints c
JOIN sys.objects o ON o.object_id = c.parent_object_id	
WHERE o.name ='Employees'
declare @Sql varchar(400)
set @Sql = 'ALTER TABLE Employees DROP CONSTRAINT ' + @CheckBirthDateConstraintName;
EXEC(@Sql);

ALTER TABLE Employees ADD CONSTRAINT Employees_Check_BirthDate CHECK (YEAR(DateOfBirth)
<= YEAR(GETDATE()) - 18 AND YEAR(DateOfBirth) >= YEAR(GETDATE()) - 60)