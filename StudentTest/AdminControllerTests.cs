using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentAttendanceManagementSystem.Controllers;
using StudentAttendanceManagementSystem.Models;
using StudentAttendanceManagementSystem.Models.DAO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace StudentTest
{
    public class AdminControllerTests
    {
        private readonly Mock<IBranchRepository> _branchRepositoryMock;
        private readonly Mock<ISubjectRepository> _subjectRepositoryMock;
        private readonly Mock<ILoginRepository> _loginRepositoryMock;
        private readonly Mock<IStudentRepository> _studentRepositoryMock;
        private readonly Mock<IFacultyRepository> _facultyRepositoryMock;
        private readonly Mock<IStudentSubjectRepository> _studentSubjectRepositoryMock;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly Mock<ISession> _sessionMock;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _branchRepositoryMock = new Mock<IBranchRepository>();
            _subjectRepositoryMock = new Mock<ISubjectRepository>();
            _loginRepositoryMock = new Mock<ILoginRepository>();
            _studentRepositoryMock = new Mock<IStudentRepository>();
            _facultyRepositoryMock = new Mock<IFacultyRepository>();
            _studentSubjectRepositoryMock = new Mock<IStudentSubjectRepository>();

            _httpContextMock = new Mock<HttpContext>();
            _sessionMock = new Mock<ISession>();

            _httpContextMock.Setup(x => x.Session).Returns(_sessionMock.Object);

            _controller = new AdminController(
                _branchRepositoryMock.Object,
                _subjectRepositoryMock.Object,
                _loginRepositoryMock.Object,
                _studentRepositoryMock.Object,
                _facultyRepositoryMock.Object,
                _studentSubjectRepositoryMock.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContextMock.Object
            };
        }

        [Fact]
        public void Admin_WhenUserIsNotAdmin_ShouldRedirectToLogin()
        {
            // Arrange
            _sessionMock.Setup(x => x.GetString("UserType")).Returns("User");

            // Act
            var result = _controller.Admin();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
        }

        [Fact]
        public void Admin_WhenUserIsAdmin_ShouldReturnViewWithBranches()
        {
            // Arrange
            _sessionMock.Setup(x => x.GetString("UserType")).Returns("Admin");
            var branches = new List<Branch> { new Branch { BranchName = "Test Branch" } };
            _branchRepositoryMock.Setup(x => x.GetAllBranch()).Returns(branches.AsQueryable());

            // Act
            var result = _controller.Admin();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Branch>>(viewResult.ViewData["branchList"]);
            Assert.Single(model);
            Assert.Equal("Test Branch", model[0].BranchName);
        }

        [Fact]
        public void AddNewBranch_WhenModelStateIsValid_ShouldRedirectToAdmin()
        {
            // Arrange
            var newBranch = new Branch { BranchName = "New Branch" };

            // Act
            var result = _controller.AddNewBranch(newBranch);

            // Assert
            _branchRepositoryMock.Verify(x => x.Add(newBranch), Times.Once);
            _sessionMock.Verify(x => x.SetString("success", "New Branch branch Added successfully!!"), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Admin", redirectToActionResult.ActionName);
            Assert.Equal("Admin", redirectToActionResult.ControllerName);
        }

        [Fact]
        public void AddNewBranch_WhenModelStateIsInvalid_ShouldRedirectToAdmin()
        {
            // Arrange
            var newBranch = new Branch { BranchName = "" };
            _controller.ModelState.AddModelError("BranchName", "Required");

            // Act
            var result = _controller.AddNewBranch(newBranch);

            // Assert
            _branchRepositoryMock.Verify(x => x.Add(It.IsAny<Branch>()), Times.Never);
            _sessionMock.Verify(x => x.SetString("error", "Please fill all details carefully"), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Admin", redirectToActionResult.ActionName);
            Assert.Equal("Admin", redirectToActionResult.ControllerName);
        }

        // Add more tests for other methods as needed
    }
}
