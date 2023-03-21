using System;

using TicketManagementSystem.Models;

namespace TicketManagementSystem.Services
{
  public interface ITicketService
  {
    void AssignTicket(int id, string username);
    int CreateTicket(string t, Priority p, string assignedTo, string desc, DateTime d, bool isPayingCustomer);
  }
}