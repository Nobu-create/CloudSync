# CloudSync — Task & Productivity Manager

A collaborative task and project management web application built for teams.


---

## 📌 Project Overview

CloudSync is a full-stack web application that allows users to create and manage tasks, collaborate on projects, assign team members, and track progress through an interactive dashboard.

Built as a Final Year Project at UITM Rzeszów, 2026.

---

## 👥 Team

| Name | Role |
|------|------|
| Nobukhosi | UI / Frontend Developer & Team Leader |
| Aliyah Sanderson | Full Stack Developer |
| Nyasha | Software Engineer & QA |

---

## 🛠️ Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 8)
- **Language:** C#
- **Database:** SQL Server + Entity Framework Core
- **Frontend:** Bootstrap 5, Chart.js, custom dark theme CSS
- **Testing:** xUnit (14 unit tests)
- **CI/CD:** GitHub Actions
- **Deployment: On-premise/Local Host

---

## ✨ Features

- User registration and login with session-based authentication
- Create, edit, and delete tasks with priority levels and due dates
- Filter tasks by category, status, and date
- Dashboard with live charts showing task progress
- Project collaboration — invite team members by email
- Notifications system for project invites and updates
- Dark theme UI throughout

---

## 🚀 How to Run Locally

1. Clone the repository:
   ```
   git clone https://github.com/Nobu-create/CloudSync.git
   ```
2. Open `taskmaster.sln` in Visual Studio 2022
3. Update the connection string in `appsettings.json` to point to your local SQL Server
4. Open Package Manager Console and run:
   ```
   Update-Database
   ```
5. Press **F5** or click the green Play button to run

---

## ✅ Tests

14 unit tests written using xUnit covering core task and model logic.

To run tests: go to **Test → Run All Tests** in Visual Studio.

---

## 📁 Project Structure

```
CloudSync/
├── taskmaster/
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── wwwroot/css/
│   └── appsettings.json
├── CloudSync.Tests/
└── .github/workflows/
```

---

*CloudSync Task & Productivity Manager | Final Year Project 2026 | UITM*
