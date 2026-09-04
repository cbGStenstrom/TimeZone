# TimeZone Installation Guide

**Product:** TimeZone - Personal Time Tracker for Software Developers

**Version:** 1.0

**Last Updated:** September 2026

---

# Purpose

This document describes how to install and configure TimeZone on a Windows workstation or server using:

- IIS
- SQL Server
- ASP.NET Core Hosting Bundle
- Blazor Server

The goal is to provide a repeatable installation process that can be used for both development and production deployments.

---

# System Requirements

## Operating System

Supported:

- Windows 10
- Windows 11
- Windows Server 2019
- Windows Server 2022

---

## .NET Runtime

Required:

```text
.NET 8 Hosting Bundle
```

Install before deployment.

---

## IIS

Required Features:

```text
Web Server (IIS)

Application Development
    .NET Extensibility
    ASP.NET Core Module

Management Tools
    IIS Management Console
```

---

## SQL Server

Supported:

```text
SQL Server Express

SQL Server Standard

SQL Server Enterprise

LocalDB (Development Only)
```

Recommended:

```text
SQL Server Express
```

for personal installations.

---

# Installation Overview

Installation consists of five steps:

1. Install SQL Server
2. Deploy Database
3. Install IIS
4. Publish Application
5. Configure Connection Strings

---

# Step 1 – Install SQL Server

Install one of:

```text
SQL Server Express
```

or

```text
SQL Server Standard
```

Verify access by connecting through:

```text
SQL Server Management Studio
```

---

# Step 2 – Deploy Database

## Create Database

Create database:

```sql
CREATE DATABASE TimeZone;
GO
```

---

## Deploy Schema

Open:

```text
TimeKeeper.Data.sqlproj
```

in Visual Studio.

Publish database project to:

```text
TimeZone
```

database.

---

## Verify Tables

Confirm the following tables exist:

```text
Laborers
Projects
WorkItems
TimeEntries
```

Future versions may rename these tables.

---

# Step 3 – Install IIS

Open:

```text
Turn Windows Features On or Off
```

Enable:

```text
Internet Information Services

World Wide Web Services

Application Development Features

Management Tools
```

---

# Step 4 – Install ASP.NET Core Hosting Bundle

Download and install:

```text
ASP.NET Core Hosting Bundle
```

for .NET 8.

Required for:

```text
Blazor Server

Kestrel

ASP.NET Core Module
```

integration with IIS.

After installation:

```text
Restart IIS
```

using:

```cmd
iisreset
```

---

# Step 5 – Create Application Folder

Recommended:

```text
C:\Applications\TimeZone
```

Example:

```text
C:\Applications\TimeZone

    appsettings.json

    TimeZone.exe

    web.config

    wwwroot\
```

---

# Step 6 – Publish Application

Open:

```text
TimeKeeper.sln
```

in Visual Studio.

Right-click:

```text
TimeKeeper.App
```

Select:

```text
Publish
```

Create profile:

```text
Folder Publish
```

Target:

```text
C:\Applications\TimeZone
```

Publish the application.

---

# Step 7 – Create Application Pool

Open:

```text
IIS Manager
```

Navigate:

```text
Application Pools
```

Create:

```text
TimeZoneAppPool
```

Settings:

```text
.NET CLR Version:
No Managed Code

Managed Pipeline:
Integrated
```

---

# Step 8 – Create IIS Site

Navigate:

```text
Sites
```

Create:

```text
TimeZone
```

Settings:

```text
Physical Path:

C:\Applications\TimeZone

Application Pool:

TimeZoneAppPool
```

Binding example:

```text
http://localhost:8080
```

---

# Step 9 – Configure Connection String

Open:

```text
appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "TimeKeeperDB":
      "Server=localhost;Database=TimeZone;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

# Step 10 – Configure Application Permissions

Grant the IIS Application Pool identity:

```text
Read

Execute
```

permissions on:

```text
C:\Applications\TimeZone
```

---

# Step 11 – Verify Startup

Browse to:

```text
http://localhost:8080
```

Expected behavior:

- Login page loads
- User can authenticate
- Dashboard loads successfully

---

# Initial User Setup

If no users exist:

1. Navigate to Login.
2. Select:

```text
Create New User
```

3. Create initial SuperUser account.

Recommended:

```text
Username:
admin
```

Change password immediately after first login.

---

# Troubleshooting

## Application Fails To Start

Check:

```text
Windows Event Viewer

Application Log
```

and

```text
stdout logs
```

if enabled.

---

## Database Connection Errors

Verify:

```text
SQL Server service running

Connection string correct

Database exists

Application Pool account has access
```

---

## 500.30 Startup Error

Verify:

```text
.NET 8 Hosting Bundle installed
```

Then:

```cmd
iisreset
```

---

## 404 Errors

Verify:

```text
Published output exists

Physical path correct

Application Pool started
```

---

# Backup Recommendations

Before upgrades:

1. Backup database.
2. Backup application folder.
3. Save current configuration.
4. Record application version.

---

# Recommended Folder Structure

```text
C:\Applications\TimeZone
│
├── Current
│
├── Backups
│
├── Logs
│
└── Releases
```

Example:

```text
Current

Releases
    1.0.0
    1.1.0
    1.2.0

Backups
    Database
    Application
```

---

# Installation Validation Checklist

## Database

- [ ] Database created
- [ ] Schema deployed
- [ ] Tables verified

## IIS

- [ ] Hosting bundle installed
- [ ] Application pool created
- [ ] Site created
- [ ] Site started

## Application

- [ ] Published successfully
- [ ] Login page loads
- [ ] Dashboard accessible
- [ ] Database connectivity verified

---

# Future Improvements

Future versions may include:

- Installer package
- Automated database deployment
- Automated upgrades
- Health checks
- Logging dashboard

---

# Version History

| Version | Description |
|----------|----------|
| 1.0 | Initial installation guide |