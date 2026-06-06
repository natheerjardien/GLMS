# TechMove General Ledger Management System (GLMS)

**Project Code:** PROG7311 POE PART 2  
**Developer:** ST10435542 - Natheer Jardien

---

## 1. Project Overview
The **General Ledger Management System (GLMS)** is a custom-built solution for TechMove to streamline client management and service contract workflows. The system focuses on data integrity, security, and automated financial calculations.

### Key Performance Indicators:
- **Security:** Role-based access control and file validation.
- **Automation:** Live API integration for currency exchange.
- **Robustness:** Full unit testing suite for all business logic.

<img width="1919" height="1043" alt="image" src="https://github.com/user-attachments/assets/3ec15681-e19b-4987-aa5d-2b22cd5123ef" />

<img width="1919" height="1043" alt="Screenshot 2026-04-22 225459" src="https://github.com/user-attachments/assets/7e1ebf3c-d14d-42ef-aaa8-4d3887030d39" />


---

## 2. Technical Stack
| Category | Technology |
| :--- | :--- |
| **Framework** | ASP.NET Core 10.0 MVC |
| **Database** | Entity Framework Core / MS SQL Server |
| **Security** | ASP.NET Core Identity |
| **Frontend** | Razor Pages & Bootswatch Morph (Neumorphic Design) |
| **Testing** | XUnit & Moq |
| **API** | Open Exchange Rates API |

## Migrations
<img width="586" height="388" alt="Screenshot 2026-04-22 231232" src="https://github.com/user-attachments/assets/bf5564aa-929a-4519-a7f8-b5303af480da" />
<img width="1483" height="501" alt="Screenshot 2026-04-22 112842" src="https://github.com/user-attachments/assets/f2538168-17e5-4a81-8d9f-3b75dedb249a" />
<img width="1580" height="867" alt="Screenshot 2026-04-22 112933" src="https://github.com/user-attachments/assets/b330fd85-6285-4602-9b50-f89562b8e4a9" />
<img width="1576" height="904" alt="Screenshot 2026-04-22 112944" src="https://github.com/user-attachments/assets/3e6c3b4e-343f-4153-a172-cd234c092137" />

---

## 3. Design Patterns
This project implements three major design patterns to ensure clean, maintainable, and scalable code:

### 3.1. Factory Pattern
Used in `ContractFactory.cs` to handle the instantiation of contracts. This ensures all new contracts start with a "Draft" status and standardized properties.

### 3.2. State Pattern
Implemented via `ContractStateManager.cs` to control the lifecycle of a contract.
- **States:** Draft, Active, On Hold, Expired.
- **Rules:** Prevents illegal moves, such as activating an expired contract.
<img width="591" height="710" alt="Screenshot 2026-04-22 230843" src="https://github.com/user-attachments/assets/d8bf0875-f8d2-4e8a-98d6-223dfe2860c0" />


<img width="1898" height="1042" alt="Screenshot 2026-04-22 230223" src="https://github.com/user-attachments/assets/0d80e97a-1766-4c7d-9b2c-612ba8b25a61" />

### 3.3. Strategy Pattern
Used for currency conversion. The app can switch between `LiveExchangeRateStrategy` (API-based) and `UsdToZarStrategy` (Hardcoded fallback) depending on internet availability.

---

## 4. Security & Validation
The system implements a "Defensive Programming" approach:
1. **Identity Management:** Two roles defined: `Admin` (Full Access) and `Staff` (Restricted Deletion).
2. **File Validation:** A custom `FileValidator` ensures only PDF files are uploaded for signed agreements, protecting the server from executable scripts (.exe).
3. **CSRF Protection:** Implemented using `[ValidateAntiForgeryToken]` on all POST actions.

<img width="1253" height="248" alt="Screenshot 2026-04-22 230634" src="https://github.com/user-attachments/assets/7cdae4db-2af7-49af-b76c-888250c3728c" />
<img width="1919" height="1036" alt="Screenshot 2026-04-22 203039" src="https://github.com/user-attachments/assets/8821b2e2-325b-4eef-a9f2-a9c93c1f09a9" />
<img width="1918" height="1046" alt="Screenshot 2026-04-22 203108" src="https://github.com/user-attachments/assets/e8ede4eb-7741-4d37-916c-b2e124e55309" />

---

## 5. Automated Test
ing
To ensure the system works as intended, a suite of Unit Tests was developed.

- **State Tests:** Validates that status transitions follow business rules.
- **API Tests:** Uses **Moq** to simulate successful and failed API responses.
- **Factory Tests:** Ensures objects are created with correct default values.

<img width="1919" height="1199" alt="Screenshot 2026-04-22 152148" src="https://github.com/user-attachments/assets/2ffa6f37-c572-4c9a-8339-5efd2a904d70" />
<img width="500" height="267" alt="Screenshot 2026-04-22 184037" src="https://github.com/user-attachments/assets/e53c43e3-79c2-4e83-b703-dc55974a5e24" />
<img width="730" height="214" alt="Screenshot 2026-04-22 181706" src="https://github.com/user-attachments/assets/04db4a5f-d57c-438e-b147-0b00b78eada1" />
<img width="1105" height="785" alt="Screenshot 2026-04-22 181730" src="https://github.com/user-attachments/assets/68f870db-7080-405a-8edf-1ae301d3785c" />
<img width="1919" height="1042" alt="Screenshot 2026-04-22 183829" src="https://github.com/user-attachments/assets/ead16605-4fc0-45e6-8844-901dd45598db" />
<img width="1919" height="1047" alt="Screenshot 2026-04-22 183850" src="https://github.com/user-attachments/assets/f34ce01e-f70f-4c59-86d0-4bf3dfc9097d" />


---

## 6. Installation & Setup
1. **Clone the repo:** `git clone [repository-url]`
2. **Database:** Update `ConnectionStrings` in `appsettings.json`.
3. **Migrations:** Run `Update-Database` in the Package Manager Console.
4. **Run:** Press `F5` in Visual Studio.
5. **Default Credentials (Seeded):**
   - **Admin:** `admin@glms.com` | `admin123`
   - **Staff:** `staff@glms.com` | `staff123`

---

## 7. User Interface (UI)
The UI uses the **Bootswatch Morph** theme, which applies a "Neumorphic" look (soft shadows and clean layers) for a modern user experience.

<img width="1883" height="997" alt="Screenshot 2026-04-22 195223" src="https://github.com/user-attachments/assets/a89a7204-febc-493c-a04b-a1a7bc72dd59" />
<img width="591" height="710" alt="Screenshot 2026-04-22 230843" src="https://github.com/user-attachments/assets/2f7bee70-1f52-408a-bd20-c7f781acc121" />

---

## 8. References
- Bootswatch (2026). *Morph Theme*. Available at: https://bootswatch.com/morph/
- Microsoft (2026). *ASP.NET Core Identity Documentation*.
- Open Exchange Rates (2026). *API Documentation*.
