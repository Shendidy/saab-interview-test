using AutoFixture;
using EmailService;
using FluentAssertions;
using NSubstitute;

using NUnit.Framework;
using System;

using TicketManagementSystem.Exceptions;
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

    string title, description, userName;
    Priority priority;
    DateTime dateTicketCreated;
    bool isPayingCustomer;
    User user;

    [SetUp]
    public void SetUp()
    {
      title = "Title";
      priority = Priority.Low;
      description = "Description";
      userName = "Mock User";
      dateTicketCreated = new DateTime(2023, 3, 22, 15, 0, 0);
      isPayingCustomer = false;

      user = _fixture.Build<User>().With(u => u.Username, userName).Create();
      _userRepository.GetUser(userName).Returns(user);
      _dateTimeProvider.DateTimeUtcNow.Returns(new DateTime(2023, 3, 22, 15, 30, 0));
    }

    [Test]
    public void CreateTicket_WhenAllParametersAreValid_ShouldCreateNewTicket()
    {
      // Arrange

      // Action
      var result = _ticketService.CreateTicket(title, priority, userName, description, dateTicketCreated, isPayingCustomer);

      // Assert
      result.GetType().Should().Be(typeof(Int32));
      _ticketRepository.Received(1).CreateTicket(Arg.Any<Ticket>());
      _emailService.Received(0).SendEmailToAdministrator(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public void CreateTicket_WhenLowPriorityAndOlderThanOneHour_ShouldCreateNewTicketWithMediumPriorityAndNotSendEmail()
    {
      //Arrange
      dateTicketCreated = new DateTime(2022, 3, 22, 14, 0, 0);

      //Act
      var result = _ticketService.CreateTicket(title, priority, userName, description, dateTicketCreated, isPayingCustomer);

      //Assert
      _ticketRepository.Received(1).CreateTicket(Arg.Is<Ticket>( t => t.Priority == Priority.Medium));
      _emailService.Received(0).SendEmailToAdministrator(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public void CreateTicket_WhenMediumPriorityAndOlderThanOneHour_ShouldCreateNewTicketWithHighPriorityAndSendEmail()
    {
      //Arrange
      priority = Priority.Medium;
      dateTicketCreated = new DateTime(2022, 3, 22, 14, 0, 0);

      //Act
      var result = _ticketService.CreateTicket(title, priority, userName, description, dateTicketCreated, isPayingCustomer);

      //Assert
      _ticketRepository.Received(1).CreateTicket(Arg.Is<Ticket>(t => t.Priority == Priority.High));
      _emailService.Received(1).SendEmailToAdministrator(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public void CreateTicket_WhenLowPriorityAndWithTitleFlag_ShouldCreateNewTicketWithMediumPriority()
    {
      //Arrange
      title = "Title with Crash flag!";

      //Act
      var result = _ticketService.CreateTicket(title, priority, userName, description, dateTicketCreated, isPayingCustomer);

      //Assert
      _ticketRepository.Received(1).CreateTicket(Arg.Is<Ticket>(t => t.Priority == Priority.Medium));
      _emailService.Received(0).SendEmailToAdministrator(Arg.Any<string>(), Arg.Any<string>());
    }
    
    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void CreateTicket_WhenNullOrWhiteSpaceArguments_ShouldThrowException(string title)
    {
      //Arrange

      //Act
      Func<object> result = () => _ticketService.CreateTicket(title, priority, userName, description, dateTicketCreated, isPayingCustomer);

      //Assert
      result.Should().Throw<InvalidTicketException>().WithMessage("Title or description were null");
    }

    [TestCase(Priority.High, 100)]
    [TestCase(Priority.Low, 50)]
    public void CreateTicket_WhenUserIsPayingCustomerAndHighPriority_ShouldHaveAccountManagerAndCorrectPrice(Priority priority, int price)
    {
      //Arrange
      isPayingCustomer = true;
      string accountManagerName = "Mock Account Manager";
      User accountManager = _fixture.Build<User>().With(u => u.Username, accountManagerName).Create();
      _userRepository.GetAccountManager().Returns(accountManager);

      //Act
      var result = _ticketService.CreateTicket(title, priority, userName, description, dateTicketCreated, isPayingCustomer);

      //Assert
      _ticketRepository.Received(1).CreateTicket(Arg.Is<Ticket>(t => t.AccountManager == accountManager && t.PriceDollars == price));
    }

    [Test]
    public void CreateTicket_WhenUserIsNonPayingCustomer_ShouldNotHaveAccountManager()
    {
      //Arrange
      _userRepository.GetAccountManager().Returns(x => null);

      //Act
      var result = _ticketService.CreateTicket(title, priority, userName, description, dateTicketCreated, isPayingCustomer);

      //Assert
      _ticketRepository.Received(1).CreateTicket(Arg.Is<Ticket>(t => t.AccountManager == null));

    }

    [TearDown]
    public void TearDown()
    {
      _ticketRepository.ClearReceivedCalls();
      _emailService.ClearReceivedCalls();
    }
  }
}
