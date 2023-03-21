using AutoFixture;
using EmailService;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TicketManagementSystem.Models;
using TicketManagementSystem.Repositories;
using TicketManagementSystem.Services;

namespace TicketManagementSystem.UnitTests
{
  public class TicketServiceTests
  {
    private readonly ITicketService _ticketService;
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly ITicketRepository _ticketRepository = Substitute.For<ITicketRepository>();

    private readonly IFixture _fixture = new Fixture();

    public TicketServiceTests()
    {
      _ticketService = new TicketService(_userRepository, _dateTimeProvider, _emailService, _ticketRepository);
    }

    [Test]
    public void CreateTicket_WhenAllParametersAreValid_ShouldCreateNewTicket()
    {
      // Arrange
      var title = "Title";
      var priority = Priority.Low;
      var description = "Description";
      var userName = "Mock User";
      var accountManagerName = "Mock Acc Manager";
      var dateTicketCreated = new DateTime(2022, 3, 22, 15, 0, 0);
      var isPayingCustomer = true;

      var user = _fixture.Build<User>().With(u => u.Username, userName).Create();
      var accountManager = _fixture.Build<User>().With(u => u.Username, accountManagerName).Create();

      _userRepository.GetUser(userName).Returns(user);
      _userRepository.GetUser(accountManagerName).Returns(accountManager);
      _dateTimeProvider.DateTimeUtcNow.Returns(new DateTime(2023, 3, 22, 15, 30, 0));

      // Action
      var result = _ticketService.CreateTicket(title, priority, userName, description, dateTicketCreated, isPayingCustomer);

      // Assert
      result.GetType().Should().Be(typeof(Int32));
      _ticketRepository.Received(1).CreateTicket(Arg.Any<Ticket>());
      _emailService.Received(0).SendEmailToAdministrator(Arg.Any<string>(), Arg.Any<string>());
    }
  }
}
