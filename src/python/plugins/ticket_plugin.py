import requests
from typing import List, Optional
from datetime import datetime
from semantic_kernel.functions import kernel_function

class Ticket:
    def __init__(self, title: str, description: str, assigned_to: Optional[str] = "Matteo Pagani", severity: Optional[str] = "Normal", status: Optional[str] = "Open", created_at: Optional[str] = None, id: Optional[str] = None):
        self.title = title
        self.description = description
        self.assignedTo = assigned_to
        self.severity = severity
        self.status = status
        self.createdAt = created_at or datetime.utcnow().isoformat()
        self.id = id

    @staticmethod
    def from_dict(data: dict):
        return Ticket(
            title=data.get("title"),
            description=data.get("description"),
            assigned_to=data.get("assignedTo"),
            severity=data.get("severity"),
            status=data.get("status"),
            created_at=data.get("createdAt"),
            id=data.get("id")
        )

    def to_dict(self):
        return {
            "title": self.title,
            "description": self.description,
            "assignedTo": self.assignedTo,
            "severity": self.severity,
            "status": self.status,
            "createdAt": self.createdAt
        }

class TicketPlugin:
    BASE_URL = "https://ticket-copilot.azurewebsites.net/api"

    def __init__(self, session: Optional[requests.Session] = None):
        self.session = session or requests.Session()

   
    def name(self) -> str:z
    
    @kernel_function
    def create_ticket(self, title: str, description: str, assigned_to: Optional[str] = "Matteo Pagani", severity: Optional[str] = "Normal", status: Optional[str] = "Open") -> Optional[Ticket]:
        """Create a new ticket with the given details."""
        ticket = Ticket(title, description, assigned_to, severity, status)
        response = self.session.post(f"{self.BASE_URL}/tickets", json=ticket.to_dict())
        if not response.ok:
            print(response.text)
            raise Exception(f"Failed to create ticket: {response.status_code} - {response.text}")
        return Ticket.from_dict(response.json())
    
    @kernel_function
    def get_tickets(self, search: Optional[str] = None, assigned_to: Optional[str] = None, status: Optional[str] = None) -> Optional[List[Ticket]]:
        """Retrieve tickets based on optional search criteria."""
        url = f"{self.BASE_URL}/tickets"
        params = {}
        if search:
            params["search"] = search
        if assigned_to:
            params["assignedTo"] = assigned_to
        if status:
            params["status"] = status
        response = self.session.get(url, params=params)
        if not response.ok:
            raise Exception(f"Failed to get tickets: {response.status_code} - {response.text}")
        return [Ticket.from_dict(item) for item in response.json()]

    @kernel_function
    def delete_ticket(self, id: str) -> None:
        """Delete a ticket by its ID."""
        response = self.session.delete(f"{self.BASE_URL}/tickets/{id}")
        if not response.ok:
            raise Exception(f"Failed to delete ticket: {response.status_code} - {response.text}")
