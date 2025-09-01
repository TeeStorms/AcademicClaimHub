# Contract Monthly Claim System (CMCS)

## Overview
The Contract Monthly Claim System (CMCS) is a prototype designed to make the monthly claim submission process easier and more transparent for Independent Contractor (IC) lecturers.  

The system allows:
- **Lecturers** to submit claims, log hours worked, and upload supporting documents.  
- **Programme Coordinators** to review, verify, and approve or decline claims.  
- **Academic Managers** to oversee claims and view overall statistics.  

This prototype focuses on the **GUI only** with no backend integration in Part 1.  

---

## Design Choices
- Built with **.NET Core WPF** for a modern and clean interface.  
- **Role-based dashboards** (Lecturer, Coordinator, Manager) for clarity.  
- **Relational database design** (documented via UML class diagram) to ensure data integrity.  
- GUI emphasizes **simplicity, accountability, and scalability**.  

---

## UML Class Diagram
The UML includes entities for:
- Lecturer  
- Claim  
- SupportingDocument  
- User  

With enumerations for **ClaimStatus** (Pending, Approved, Declined) and **Role** (Lecturer, Coordinator, Manager).  
The UML ensures proper key relationships and future-proof database integration.  

---

## Project Plan (Part 1)
- **Day 2**: Requirements finalized  
- **Day 4**: UML class diagram completed  
- **Day 7**: GUI wireframes finalized  
- **Day 11**: WPF prototype completed  
- **Day 13**: Documentation updated  
- **Day 14**: Final review and submission  

---

## Version Control (GitHub)
Development was managed with GitHub commits to ensure a clear history of progress:

- **Initial Commit**: Created project repository and base structure.  
- **UML Commit**: Added UML class diagram and database design.  
- **Lecturer Dashboard Commit**: Implemented Lecturer dashboard and navigation.  
- **Coordinator & Manager Dashboards Commit**: Added dashboards for Programme Coordinator and Academic Manager.  
- **Final Commit**: Added comments to all `.xaml` and `.cs` files, updated documentation, and finalized prototype.  

---

## How to Run
1. Clone the repository.  
2. Open the solution in **Visual Studio** (tested on VS 2022).  
3. Run the project to explore the GUI prototype.  

---

## Notes
- This submission represents **Part 1** (prototype). Backend and database integration will follow in later phases.  
- **AI Disclosure**: ChatGPT was consulted for planning, brainstorming, and proofreading only. All coding, design, and analysis are my own.  

