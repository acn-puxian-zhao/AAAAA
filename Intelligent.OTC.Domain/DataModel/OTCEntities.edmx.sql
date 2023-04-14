
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 06/30/2015 22:12:42
-- Generated from EDMX file: C:\_Works\OTC Source\trunk\2 Development\2_4 Code\OTC.POC.Domain\DataModel\OTCEntities.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [OTC];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_CONTACTER_CUSTOMER]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[T_CONTACTER] DROP CONSTRAINT [FK_CONTACTER_CUSTOMER];
GO
IF OBJECT_ID(N'[dbo].[FK_CUSTOMER_BILL_GROUP_CFG]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[T_CUSTOMER] DROP CONSTRAINT [FK_CUSTOMER_BILL_GROUP_CFG];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[FK_CUSTOMER_LEVEL_CFG_CUSTOMER]', 'F') IS NOT NULL
    ALTER TABLE [OTCModelStoreContainer].[T_CUSTOMER_LEVEL_CFG] DROP CONSTRAINT [FK_CUSTOMER_LEVEL_CFG_CUSTOMER];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[FK_SYS_ROLE_FUNC_SYS_FUNC]', 'F') IS NOT NULL
    ALTER TABLE [OTCModelStoreContainer].[T_SYS_ROLE_FUNC] DROP CONSTRAINT [FK_SYS_ROLE_FUNC_SYS_FUNC];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[FK_SYS_ROLE_FUNC_SYS_ROLE]', 'F') IS NOT NULL
    ALTER TABLE [OTCModelStoreContainer].[T_SYS_ROLE_FUNC] DROP CONSTRAINT [FK_SYS_ROLE_FUNC_SYS_ROLE];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[FK_SYS_TYPE_DETAIL_SYS_TYPE]', 'F') IS NOT NULL
    ALTER TABLE [OTCModelStoreContainer].[T_SYS_TYPE_DETAIL] DROP CONSTRAINT [FK_SYS_TYPE_DETAIL_SYS_TYPE];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[FK_SYS_USER_ROLE_SYS_ROLE]', 'F') IS NOT NULL
    ALTER TABLE [OTCModelStoreContainer].[T_SYS_USER_ROLE] DROP CONSTRAINT [FK_SYS_USER_ROLE_SYS_ROLE];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[FK_SYS_USER_ROLE_SYS_USER]', 'F') IS NOT NULL
    ALTER TABLE [OTCModelStoreContainer].[T_SYS_USER_ROLE] DROP CONSTRAINT [FK_SYS_USER_ROLE_SYS_USER];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[T_BILL_GROUP_CFG]', 'U') IS NOT NULL
    DROP TABLE [dbo].[T_BILL_GROUP_CFG];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[T_CONTACT_HISTORY]', 'U') IS NOT NULL
    DROP TABLE [OTCModelStoreContainer].[T_CONTACT_HISTORY];
GO
IF OBJECT_ID(N'[dbo].[T_CONTACTER]', 'U') IS NOT NULL
    DROP TABLE [dbo].[T_CONTACTER];
GO
IF OBJECT_ID(N'[dbo].[T_CUSTOMER]', 'U') IS NOT NULL
    DROP TABLE [dbo].[T_CUSTOMER];
GO
IF OBJECT_ID(N'[dbo].[T_CUSTOMER_AGING]', 'U') IS NOT NULL
    DROP TABLE [dbo].[T_CUSTOMER_AGING];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[T_CUSTOMER_LEVEL_CFG]', 'U') IS NOT NULL
    DROP TABLE [OTCModelStoreContainer].[T_CUSTOMER_LEVEL_CFG];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[T_REMINDER]', 'U') IS NOT NULL
    DROP TABLE [OTCModelStoreContainer].[T_REMINDER];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[T_SOA_HISTORY]', 'U') IS NOT NULL
    DROP TABLE [OTCModelStoreContainer].[T_SOA_HISTORY];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[T_SYS_CONFIG]', 'U') IS NOT NULL
    DROP TABLE [OTCModelStoreContainer].[T_SYS_CONFIG];
GO
IF OBJECT_ID(N'[dbo].[T_SYS_FUNC]', 'U') IS NOT NULL
    DROP TABLE [dbo].[T_SYS_FUNC];
GO
IF OBJECT_ID(N'[dbo].[T_SYS_ROLE]', 'U') IS NOT NULL
    DROP TABLE [dbo].[T_SYS_ROLE];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[T_SYS_ROLE_FUNC]', 'U') IS NOT NULL
    DROP TABLE [OTCModelStoreContainer].[T_SYS_ROLE_FUNC];
GO
IF OBJECT_ID(N'[dbo].[T_SYS_TYPE]', 'U') IS NOT NULL
    DROP TABLE [dbo].[T_SYS_TYPE];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[T_SYS_TYPE_DETAIL]', 'U') IS NOT NULL
    DROP TABLE [OTCModelStoreContainer].[T_SYS_TYPE_DETAIL];
GO
IF OBJECT_ID(N'[dbo].[T_SYS_USER]', 'U') IS NOT NULL
    DROP TABLE [dbo].[T_SYS_USER];
GO
IF OBJECT_ID(N'[OTCModelStoreContainer].[T_SYS_USER_ROLE]', 'U') IS NOT NULL
    DROP TABLE [OTCModelStoreContainer].[T_SYS_USER_ROLE];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'BillGroupCfgs'
CREATE TABLE [dbo].[BillGroupCfgs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [BillGroupCode] varchar(50)  NOT NULL,
    [BillGroupName] nvarchar(50)  NULL,
    [OneYearSales] decimal(12,4)  NULL
);
GO

-- Creating table 'Customers'
CREATE TABLE [dbo].[Customers] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CustomerNum] varchar(50)  NOT NULL,
    [CustomerName] nvarchar(50)  NULL,
    [SiteCode] varchar(50)  NOT NULL,
    [SiteName] varchar(50)  NULL,
    [BillGroupCode] varchar(50)  NULL,
    [Collector] nvarchar(50)  NULL,
    [Sales] nvarchar(50)  NULL,
    [SpecialNotes] nvarchar(3000)  NULL,
    [PayCycleType] varchar(50)  NULL,
    [PAY_CYCLE_VALUE] varchar(50)  NULL,
    [ICFlg] char(1)  NULL,
    [IsValidFlg] char(1)  NULL,
    [AutoReminderFlg] char(1)  NULL
);
GO

-- Creating table 'Contacters'
CREATE TABLE [dbo].[Contacters] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CustomerNum] varchar(50)  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Title] nvarchar(100)  NULL,
    [Department] nvarchar(100)  NULL,
    [Number] varchar(50)  NULL,
    [EmailAddress] varchar(50)  NULL,
    [LivingAddress] varchar(50)  NULL,
    [Country] varchar(20)  NULL
);
GO

-- Creating table 'ContactHistories'
CREATE TABLE [dbo].[ContactHistories] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CustomerNum] varchar(50)  NOT NULL,
    [ContactType] varchar(20)  NOT NULL,
    [RPCFlg] char(1)  NULL,
    [Collector] nvarchar(50)  NOT NULL,
    [Contacter] nvarchar(50)  NOT NULL,
    [ContactDate] datetime  NOT NULL,
    [ReminderDate] datetime  NULL,
    [Comments] nvarchar(500)  NULL
);
GO

-- Creating table 'CustomerLevelCfgs'
CREATE TABLE [dbo].[CustomerLevelCfgs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CustomerNum] varchar(50)  NOT NULL,
    [RistScore] int  NOT NULL,
    [UpdatedBy] varchar(50)  NOT NULL,
    [UpdatedDate] datetime  NOT NULL,
    [Comment] nvarchar(500)  NULL
);
GO

-- Creating table 'CustomerAgings'
CREATE TABLE [dbo].[CustomerAgings] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [SiteCode] nvarchar(50)  NOT NULL,
    [CustomerNum] nvarchar(50)  NOT NULL,
    [CustomerName] nvarchar(100)  NOT NULL,
    [BillGroupCode] nvarchar(50)  NOT NULL,
    [BillGroupName] nvarchar(50)  NOT NULL,
    [Country] nvarchar(20)  NOT NULL,
    [CreditTrem] nvarchar(20)  NOT NULL,
    [CreditLimit] decimal(18,5)  NOT NULL,
    [Collector] nvarchar(50)  NOT NULL,
    [TotalAmt] decimal(18,5)  NOT NULL,
    [CurrentAmt] decimal(18,5)  NOT NULL,
    [Due30Days] decimal(18,5)  NOT NULL,
    [Due60Days] decimal(18,5)  NOT NULL,
    [Due90Days] decimal(18,5)  NOT NULL,
    [Due180Days] decimal(18,5)  NOT NULL,
    [Due360Days] decimal(18,5)  NOT NULL,
    [DueOver360Days] decimal(18,5)  NOT NULL,
    [DueOver60Days] decimal(18,5)  NOT NULL,
    [CreateDate] datetime  NOT NULL
);
GO

-- Creating table 'Reminders'
CREATE TABLE [dbo].[Reminders] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CustomerNum] varchar(50)  NOT NULL,
    [FromAddress] varchar(50)  NULL,
    [ToAddress] varchar(50)  NULL,
    [Subject] varchar(50)  NULL,
    [RemindDate] datetime  NULL,
    [Language] varchar(10)  NULL,
    [Round] int  NULL,
    [SentFlg] char(1)  NULL,
    [Errors] nvarchar(1000)  NULL
);
GO

-- Creating table 'T_SOA_HISTORY'
CREATE TABLE [dbo].[T_SOA_HISTORY] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [CUSTOMER_NUM] varchar(50)  NOT NULL,
    [SOA_SUBJECT] nvarchar(100)  NULL,
    [COLLECTOR] nvarchar(50)  NULL,
    [SOA_TEMPLATE] varchar(50)  NULL,
    [REMINDER_ID] int  NULL
);
GO

-- Creating table 'T_SYS_CONFIG'
CREATE TABLE [dbo].[T_SYS_CONFIG] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [CFG_NAME] nvarchar(50)  NULL,
    [CFG_VALUE] nvarchar(50)  NULL,
    [CFG_VALUE2] nvarchar(50)  NULL
);
GO

-- Creating table 'SysFuncs'
CREATE TABLE [dbo].[SysFuncs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FuncId] varchar(50)  NOT NULL,
    [FuncName] nvarchar(50)  NULL,
    [FuncPage] varchar(50)  NULL,
    [Parent] varchar(50)  NULL
);
GO

-- Creating table 'SysRoles'
CREATE TABLE [dbo].[SysRoles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoleId] varchar(50)  NOT NULL,
    [RoleName] nvarchar(50)  NULL
);
GO

-- Creating table 'SysRoleFuncs'
CREATE TABLE [dbo].[SysRoleFuncs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoleId] varchar(50)  NULL,
    [FuncId] varchar(50)  NULL
);
GO

-- Creating table 'T_SYS_TYPE'
CREATE TABLE [dbo].[T_SYS_TYPE] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [TYPE_CODE] varchar(20)  NOT NULL,
    [TYPE_NAME] varchar(50)  NULL,
    [DESCRIPTION] nvarchar(500)  NULL
);
GO

-- Creating table 'T_SYS_TYPE_DETAIL'
CREATE TABLE [dbo].[T_SYS_TYPE_DETAIL] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [TYPE_CODE] varchar(20)  NOT NULL,
    [DETAIL_NAME] nvarchar(50)  NOT NULL,
    [DETAIL_VALUE] nvarchar(50)  NULL,
    [DETAIL_VALUE2] nvarchar(50)  NULL,
    [DETAIL_VALUE3] nvarchar(500)  NULL,
    [DESCRIPTION] nvarchar(500)  NULL
);
GO

-- Creating table 'SysUsers'
CREATE TABLE [dbo].[SysUsers] (
    [Id] int  NOT NULL,
    [EID] varchar(50)  NOT NULL,
    [Name] nvarchar(50)  NULL,
    [Email] varchar(50)  NULL,
    [Role] varchar(50)  NULL,
    [Team] varchar(50)  NULL
);
GO

-- Creating table 'SysUserRoles'
CREATE TABLE [dbo].[SysUserRoles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [EID] varchar(50)  NOT NULL,
    [RoleId] varchar(50)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [BillGroupCode] in table 'BillGroupCfgs'
ALTER TABLE [dbo].[BillGroupCfgs]
ADD CONSTRAINT [PK_BillGroupCfgs]
    PRIMARY KEY CLUSTERED ([BillGroupCode] ASC);
GO

-- Creating primary key on [CustomerNum] in table 'Customers'
ALTER TABLE [dbo].[Customers]
ADD CONSTRAINT [PK_Customers]
    PRIMARY KEY CLUSTERED ([CustomerNum] ASC);
GO

-- Creating primary key on [Id] in table 'Contacters'
ALTER TABLE [dbo].[Contacters]
ADD CONSTRAINT [PK_Contacters]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id], [CustomerNum], [ContactType], [Collector], [Contacter], [ContactDate] in table 'ContactHistories'
ALTER TABLE [dbo].[ContactHistories]
ADD CONSTRAINT [PK_ContactHistories]
    PRIMARY KEY CLUSTERED ([Id], [CustomerNum], [ContactType], [Collector], [Contacter], [ContactDate] ASC);
GO

-- Creating primary key on [Id], [CustomerNum], [RistScore], [UpdatedBy], [UpdatedDate] in table 'CustomerLevelCfgs'
ALTER TABLE [dbo].[CustomerLevelCfgs]
ADD CONSTRAINT [PK_CustomerLevelCfgs]
    PRIMARY KEY CLUSTERED ([Id], [CustomerNum], [RistScore], [UpdatedBy], [UpdatedDate] ASC);
GO

-- Creating primary key on [Id] in table 'CustomerAgings'
ALTER TABLE [dbo].[CustomerAgings]
ADD CONSTRAINT [PK_CustomerAgings]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id], [CustomerNum] in table 'Reminders'
ALTER TABLE [dbo].[Reminders]
ADD CONSTRAINT [PK_Reminders]
    PRIMARY KEY CLUSTERED ([Id], [CustomerNum] ASC);
GO

-- Creating primary key on [ID], [CUSTOMER_NUM] in table 'T_SOA_HISTORY'
ALTER TABLE [dbo].[T_SOA_HISTORY]
ADD CONSTRAINT [PK_T_SOA_HISTORY]
    PRIMARY KEY CLUSTERED ([ID], [CUSTOMER_NUM] ASC);
GO

-- Creating primary key on [ID] in table 'T_SYS_CONFIG'
ALTER TABLE [dbo].[T_SYS_CONFIG]
ADD CONSTRAINT [PK_T_SYS_CONFIG]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [FuncId] in table 'SysFuncs'
ALTER TABLE [dbo].[SysFuncs]
ADD CONSTRAINT [PK_SysFuncs]
    PRIMARY KEY CLUSTERED ([FuncId] ASC);
GO

-- Creating primary key on [RoleId] in table 'SysRoles'
ALTER TABLE [dbo].[SysRoles]
ADD CONSTRAINT [PK_SysRoles]
    PRIMARY KEY CLUSTERED ([RoleId] ASC);
GO

-- Creating primary key on [Id] in table 'SysRoleFuncs'
ALTER TABLE [dbo].[SysRoleFuncs]
ADD CONSTRAINT [PK_SysRoleFuncs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [TYPE_CODE] in table 'T_SYS_TYPE'
ALTER TABLE [dbo].[T_SYS_TYPE]
ADD CONSTRAINT [PK_T_SYS_TYPE]
    PRIMARY KEY CLUSTERED ([TYPE_CODE] ASC);
GO

-- Creating primary key on [ID], [TYPE_CODE], [DETAIL_NAME] in table 'T_SYS_TYPE_DETAIL'
ALTER TABLE [dbo].[T_SYS_TYPE_DETAIL]
ADD CONSTRAINT [PK_T_SYS_TYPE_DETAIL]
    PRIMARY KEY CLUSTERED ([ID], [TYPE_CODE], [DETAIL_NAME] ASC);
GO

-- Creating primary key on [EID] in table 'SysUsers'
ALTER TABLE [dbo].[SysUsers]
ADD CONSTRAINT [PK_SysUsers]
    PRIMARY KEY CLUSTERED ([EID] ASC);
GO

-- Creating primary key on [Id], [EID], [RoleId] in table 'SysUserRoles'
ALTER TABLE [dbo].[SysUserRoles]
ADD CONSTRAINT [PK_SysUserRoles]
    PRIMARY KEY CLUSTERED ([Id], [EID], [RoleId] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [BillGroupCode] in table 'Customers'
ALTER TABLE [dbo].[Customers]
ADD CONSTRAINT [FK_CUSTOMER_BILL_GROUP_CFG]
    FOREIGN KEY ([BillGroupCode])
    REFERENCES [dbo].[BillGroupCfgs]
        ([BillGroupCode])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CUSTOMER_BILL_GROUP_CFG'
CREATE INDEX [IX_FK_CUSTOMER_BILL_GROUP_CFG]
ON [dbo].[Customers]
    ([BillGroupCode]);
GO

-- Creating foreign key on [CustomerNum] in table 'Contacters'
ALTER TABLE [dbo].[Contacters]
ADD CONSTRAINT [FK_CONTACTER_CUSTOMER]
    FOREIGN KEY ([CustomerNum])
    REFERENCES [dbo].[Customers]
        ([CustomerNum])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CONTACTER_CUSTOMER'
CREATE INDEX [IX_FK_CONTACTER_CUSTOMER]
ON [dbo].[Contacters]
    ([CustomerNum]);
GO

-- Creating foreign key on [CustomerNum] in table 'CustomerLevelCfgs'
ALTER TABLE [dbo].[CustomerLevelCfgs]
ADD CONSTRAINT [FK_CUSTOMER_LEVEL_CFG_CUSTOMER]
    FOREIGN KEY ([CustomerNum])
    REFERENCES [dbo].[Customers]
        ([CustomerNum])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CUSTOMER_LEVEL_CFG_CUSTOMER'
CREATE INDEX [IX_FK_CUSTOMER_LEVEL_CFG_CUSTOMER]
ON [dbo].[CustomerLevelCfgs]
    ([CustomerNum]);
GO

-- Creating foreign key on [FuncId] in table 'SysRoleFuncs'
ALTER TABLE [dbo].[SysRoleFuncs]
ADD CONSTRAINT [FK_SYS_ROLE_FUNC_SYS_FUNC]
    FOREIGN KEY ([FuncId])
    REFERENCES [dbo].[SysFuncs]
        ([FuncId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SYS_ROLE_FUNC_SYS_FUNC'
CREATE INDEX [IX_FK_SYS_ROLE_FUNC_SYS_FUNC]
ON [dbo].[SysRoleFuncs]
    ([FuncId]);
GO

-- Creating foreign key on [RoleId] in table 'SysRoleFuncs'
ALTER TABLE [dbo].[SysRoleFuncs]
ADD CONSTRAINT [FK_SYS_ROLE_FUNC_SYS_ROLE]
    FOREIGN KEY ([RoleId])
    REFERENCES [dbo].[SysRoles]
        ([RoleId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SYS_ROLE_FUNC_SYS_ROLE'
CREATE INDEX [IX_FK_SYS_ROLE_FUNC_SYS_ROLE]
ON [dbo].[SysRoleFuncs]
    ([RoleId]);
GO

-- Creating foreign key on [RoleId] in table 'SysUserRoles'
ALTER TABLE [dbo].[SysUserRoles]
ADD CONSTRAINT [FK_SYS_USER_ROLE_SYS_ROLE]
    FOREIGN KEY ([RoleId])
    REFERENCES [dbo].[SysRoles]
        ([RoleId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SYS_USER_ROLE_SYS_ROLE'
CREATE INDEX [IX_FK_SYS_USER_ROLE_SYS_ROLE]
ON [dbo].[SysUserRoles]
    ([RoleId]);
GO

-- Creating foreign key on [TYPE_CODE] in table 'T_SYS_TYPE_DETAIL'
ALTER TABLE [dbo].[T_SYS_TYPE_DETAIL]
ADD CONSTRAINT [FK_SYS_TYPE_DETAIL_SYS_TYPE]
    FOREIGN KEY ([TYPE_CODE])
    REFERENCES [dbo].[T_SYS_TYPE]
        ([TYPE_CODE])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SYS_TYPE_DETAIL_SYS_TYPE'
CREATE INDEX [IX_FK_SYS_TYPE_DETAIL_SYS_TYPE]
ON [dbo].[T_SYS_TYPE_DETAIL]
    ([TYPE_CODE]);
GO

-- Creating foreign key on [EID] in table 'SysUserRoles'
ALTER TABLE [dbo].[SysUserRoles]
ADD CONSTRAINT [FK_SYS_USER_ROLE_SYS_USER]
    FOREIGN KEY ([EID])
    REFERENCES [dbo].[SysUsers]
        ([EID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SYS_USER_ROLE_SYS_USER'
CREATE INDEX [IX_FK_SYS_USER_ROLE_SYS_USER]
ON [dbo].[SysUserRoles]
    ([EID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------