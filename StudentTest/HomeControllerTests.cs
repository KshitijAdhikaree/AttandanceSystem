using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentAttendanceManagementSystem.Controllers;
using StudentAttendanceManagementSystem.Models;
using StudentAttendanceManagementSystem.Models.DAO;
using System;
using System.Collections.Generic;
using Xunit;

namespace StudentTest
{
    public class HomeControllerTests
    {
        private readonly Mock<ILoginRepository> _mockLoginRepo;
        private readonly Mock<IAdminRepository> _mockAdminRepo;
        private readonly Mock<IFacultyRepository> _mockFacultyRepo;
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<IBranchRepository> _mockBranchRepo;
        private readonly Mock<IStudentSubjectRepository> _mockStudentSubjectRepo;
        private readonly Mock<ISubjectRepository> _mockSubjectRepo;
        private readonly HomeController _controller;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<IResponseCookies> _mockResponseCookies;
        private readonly Mock<IRequestCookieCollection> _mockRequestCookies;

        public HomeControllerTests()
        {
            _mockLoginRepo = new Mock<ILoginRepository>();
            _mockAdminRepo = new Mock<IAdminRepository>();
            _mockFacultyRepo = new Mock<IFacultyRepository>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockBranchRepo = new Mock<IBranchRepository>();
            _mockStudentSubjectRepo = new Mock<IStudentSubjectRepository>();
            _mockSubjectRepo = new Mock<ISubjectRepository>();

            _mockHttpContext = new Mock<HttpContext>();
            _mockSession = new Mock<ISession>();
            _mockResponseCookies = new Mock<IResponseCookies>();
            _mockRequestCookies = new Mock<IRequestCookieCollection>();

            // Set up mock session
            _mockHttpContext.SetupGet(c => c.Session).Returns(_mockSession.Object);

            // Set up mock response cookies
            _mockHttpContext.SetupGet(c => c.Response.Cookies).Returns(_mockResponseCookies.Object);

            // Set up mock request cookies
            _mockHttpContext.SetupGet(c => c.Request.Cookies).Returns(_mockRequestCookies.Object);

            _controller = new HomeController(
                _mockLoginRepo.Object,
                _mockAdminRepo.Object,
                _mockFacultyRepo.Object,
                _mockStudentRepo.Object,
                _mockBranchRepo.Object,
                _mockStudentSubjectRepo.Object,
                _mockSubjectRepo.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        [Fact]
        public void Login_Post_ReturnsRedirectToAction_WhenLoginIsValid()
        {
            // Arrange
            var login = new Login { Username = "test", Password = "password", remember = true };
            var userLogin = new Login { Id = 1 };
            var admin = new Admin { Id = 1, Name = "AdminName" };

            _mockLoginRepo.Setup(repo => repo.GetLogin(login.Username, login.Password)).Returns(userLogin);
            _mockAdminRepo.Setup(repo => repo.GetAdminByLoginId(userLogin.Id)).Returns(admin);

            // Simulate setting cookies
            _mockResponseCookies.Setup(c => c.Append("UserType", "Admin", It.IsAny<CookieOptions>()));
            _mockResponseCookies.Setup(c => c.Append("AdminId", admin.Id.ToString(), It.IsAny<CookieOptions>()));

            // Act
            var result = _controller.Login(login) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.ActionName);
            Assert.Equal("Admin", result.ControllerName);
            _mockResponseCookies.Verify(c => c.Append("UserType", "Admin", It.IsAny<CookieOptions>()), Times.Once);
            _mockResponseCookies.Verify(c => c.Append("AdminId", admin.Id.ToString(), It.IsAny<CookieOptions>()), Times.Once);
        }

        [Fact]
        public void Logout_ClearsSessionAndCookies()
        {
            // Arrange
            var cookieOptions = new CookieOptions();

            // Act
            var result = _controller.Logout() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            _mockSession.Verify(s => s.Remove("UserType"), Times.Once);
            _mockSession.Verify(s => s.Remove("AdminId"), Times.Once);
            _mockSession.Verify(s => s.Remove("FacultyId"), Times.Once);
            _mockSession.Verify(s => s.Remove("StudentId"), Times.Once);
            _mockResponseCookies.Verify(c => c.Delete("UserType"), Times.Once);
            _mockResponseCookies.Verify(c => c.Delete("AdminId"), Times.Once);
            _mockResponseCookies.Verify(c => c.Delete("FacultyId"), Times.Once);
            _mockResponseCookies.Verify(c => c.Delete("StudentId"), Times.Once);
        }
    }
}
