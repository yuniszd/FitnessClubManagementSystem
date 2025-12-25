# Fitness Club Management System

## Overview
A robust **Gym Management API** for handling members, subscriptions, payments, reports, and asynchronous tasks. Designed with **scalability, modularity, and maintainability** in mind.  

## Key Features
- **Member Management:** CRUD operations, subscription tracking, QR code validation.  
- **Subscription & Payment Handling:** Create, update, renew subscriptions; track payments.  
- **Reporting & Analytics:** Admin and reception reports, quick stats dashboard.  
- **Event-Driven Architecture:** RabbitMQ integration for asynchronous processing.  
- **Background Jobs:** Hangfire-powered recurring jobs for subscription reminders.  
- **Middleware & Security:** JWT authentication, role-based authorization, input validation.  
- **Paging & Filtering:** Efficient API endpoints with pagination and search support.  
- **Logging & Error Handling:** Structured logging with comprehensive exception management.  

## Technologies
- **.NET 6 / C#**  
- **Entity Framework Core** (SQL Server)  
- **RabbitMQ** for messaging  
- **Hangfire** for background jobs  
- **FluentValidation** for DTO validation  
- **Swagger / OpenAPI** for API documentation  

## Architecture Highlights
- **Clean Architecture:** Separation of concerns with services, repositories, DTOs, and events.  
- **Dependency Injection:** All services and consumers are DI-ready for testability.  
- **Event-Driven Design:** Decoupled messaging between services using RabbitMQ.  
- **Scalable & Maintainable:** Modular service design, supports future extensions easily.  

## Usage
1. Configure `appsettings.json` with **DB connection**, **JWT**, **SMTP**, and **RabbitMQ**.  
2. Run the API; Swagger UI available at `/swagger`.  
3. Background jobs (subscription reminders) run automatically via Hangfire dashboard `/hangfire`.  
4. Use endpoints with proper role authorization (`Admin`, `Reception`).  

## Clone the Repository
To clone this project to your local machine, run the following command:

```bash
git clone https://github.com/yuniszd/FitnessClubManagementSystem.git
