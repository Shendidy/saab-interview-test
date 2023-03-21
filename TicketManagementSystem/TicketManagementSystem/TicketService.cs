using System;
using EmailService;
using TicketManagementSystem.Models;
using TicketManagementSystem.Services;
using TicketManagementSystem.Exceptions;
using TicketManagementSystem.Repositories;
using System.Linq;

namespace TicketManagementSystem
{
  public class TicketService : ITicketService
  {
    private readonly IUserRepository _userRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailService _emailService;
    private readonly ITicketRepository _ticketRepository;

    public TicketService() : this(
      new UserRepository(),
      new DateTimeProvider(),
      new EmailServiceProxy(),
      new TicketRepositoryProxy())
    {
    }

    public TicketService(
      IUserRepository userRepository,
      IDateTimeProvider dateTimeProvider,
      IEmailService emailService,
      ITicketRepository ticketRepository)
    {
      _userRepository = userRepository;
      _dateTimeProvider = dateTimeProvider;
      _emailService = emailService;
      _ticketRepository = ticketRepository;
    }

    public int CreateTicket(string t, Priority p, string assignedTo, string desc, DateTime d, bool isPayingCustomer)
    {
      // TODO
      // Move validations to a separate class
      if (String.IsNullOrWhiteSpace(t) || String.IsNullOrWhiteSpace(desc))
      {
        throw new InvalidTicketException("Title or description were null");
      }

      var user = !String.IsNullOrWhiteSpace(assignedTo) ?
        GetUser(assignedTo) :
        throw new UnknownUserException("User " + assignedTo + " not found");

      var ticketTitleFlags = Enum.GetNames(typeof(TicketTitleFlags));

      if ((d < _dateTimeProvider.DateTimeUtcNow - TimeSpan.FromHours(1) ||
        ticketTitleFlags.Any(f => t.Contains(f))) &&
        p > Priority.High)
      {
        p--;
      }

      if (p .Equals(Priority.High))
      {
        _emailService.SendEmailToAdministrator(t, assignedTo);
      }

      double price = 0;
      User? accountManager = _userRepository.GetAccountManager();
      if (isPayingCustomer && accountManager is not null)
      {
        price = p.Equals(Priority.High) ? 100 : 50;
      }

      var ticket = new Ticket()
      {
        Title = t,
        AssignedUser = user,
        Priority = p,
        Description = desc,
        Created = d,
        PriceDollars = price,
        AccountManager = accountManager
      };

      var id = _ticketRepository.CreateTicket(ticket);

      return id;
    }

    public void AssignTicket(int id, string username)
    {
      var user = GetUser(username);
      var ticket = GetTicket(id);

      ticket.AssignedUser = user;

      _ticketRepository.UpdateTicket(ticket);
    }

    private Ticket GetTicket(int id)
    {
      var ticket = _ticketRepository.GetTicket(id);

      if (ticket is null)
      {
        throw new ApplicationException("No ticket found for id " + id);
      }

      return ticket;
    }

    private User GetUser(string username)
    {
      var user = _userRepository.GetUser(username);
      if(user is null) throw new UnknownUserException("User " + username + " not found");
      return user;
    }
  }
}
