using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentAttendanceManagementSystem.Controllers;
using StudentAttendanceManagementSystem.Models;
using StudentAttendanceManagementSystem.Models.DAO;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace StudentTest
{
    public class FacultyControllerTests
    {
        private readonly Mock<IBranchRepository> _mockBranchRepo;
        private readonly Mock<ISubjectRepository> _mockSubjectRepo;
        private readonly Mock<ILoginRepository> _mockLoginRepo;
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<IFacultyRepository> _mockFacultyRepo;
        private readonly Mock<IStudentSubjectRepository> _mockStudentSubjectRepo;
        private readonly FacultyController _controller;
        private readonly DefaultHttpContext _httpContext;

        public FacultyControllerTests()
        {
            _mockBranchRepo = new Mock<IBranchRepository>();
            _mockSubjectRepo = new Mock<ISubjectRepository>();
            _mockLoginRepo = new Mock<ILoginRepository>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockFacultyRepo = new Mock<IFacultyRepository>();
            _mockStudentSubjectRepo = new Mock<IStudentSubjectRepository>();

            _httpContext = new DefaultHttpContext();
            _controller = new FacultyController(
                _mockBranchRepo.Object,
                _mockSubjectRepo.Object,
                _mockLoginRepo.Object,
                _mockStudentRepo.Object,
                _mockFacultyRepo.Object,
                _mockStudentSubjectRepo.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                }
            };
        }

        [Fact]
        public void Faculty_ReturnsViewResult_WithValidData()
        {
            // Arrange
            int facultyId = 1;
            _httpContext.Session.SetInt32("FacultyId", facultyId);
            _httpContext.Session.SetString("UserType", "Faculty");

            var faculty = new Faculty
            {
                Id = facultyId,
                SubjectId = 1
            };
            var subject = new Subject
            {
                Id = 1,
                SubjectName = "Math"
            };
            var studentSubjects = new List<StudentSubject>
            {
                new StudentSubject
                {
                    Id = 1,
                    StudentId = 1,
                    SubjectId = 1
                }
            };
            var student = new Student
            {
                Id = 1,
                FirstName = "John"
            };

            _mockFacultyRepo.Setup(repo => repo.GetById(facultyId)).Returns(faculty);
            _mockSubjectRepo.Setup(repo => repo.GetById(faculty.SubjectId)).Returns(subject);
            _mockStudentSubjectRepo.Setup(repo => repo.GetBySubjectId(faculty.SubjectId)).Returns(studentSubjects);
            _mockStudentRepo.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(student);

            // Act
            var result = _controller.Faculty() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(subject, result.ViewData["Subject"]);
            Assert.Equal(faculty, result.ViewData["Faculty"]);
            Assert.Single(result.ViewData["StudentSubjectList"] as List<StudentSubject>);
        }

        [Fact]
        public void UpdateAttendance_ReturnsViewResult_WithValidData()
        {
            // Arrange
            int objId = 1;
            var studentSubject = new StudentSubject { Id = objId, StudentId = 1 };
            var student = new Student { Id = 1, FirstName = "John" };

            _mockStudentSubjectRepo.Setup(repo => repo.GetById(objId)).Returns(studentSubject);
            _mockStudentRepo.Setup(repo => repo.GetById(studentSubject.StudentId)).Returns(student);

            // Act
            var result = _controller.UpdateAttendance(objId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(student, result.ViewData["StudentObj"]);
            Assert.Equal(studentSubject, result.ViewData["StuSubObj"]);
        }

        [Fact]
        public void UpdateAttendance_ReturnsRedirectToAction_WhenStudentSubjectNotFound()
        {
            // Arrange
            int objId = 1;
            _mockStudentSubjectRepo.Setup(repo => repo.GetById(objId)).Returns((StudentSubject)null);

            // Act
            var result = _controller.UpdateAttendance(objId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Faculty", result.ActionName);
            Assert.Equal("Faculty", result.ControllerName);
        }

        [Fact]
        public void Update_ReturnsRedirectToAction_WhenModelStateIsValid()
        {
            // Arrange
            var studentSubject = new StudentSubject { Id = 1 };
            _controller.ModelState.Clear(); // Ensure ModelState is valid
            _mockStudentSubjectRepo.Setup(repo => repo.Update(studentSubject));

            // Act
            var result = _controller.Update(studentSubject) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UpdateAttendance", result.ActionName);
            Assert.Equal("Faculty", result.ControllerName);
            Assert.Equal(studentSubject.Id, result.RouteValues["objId"]);
            Assert.Equal("Attendance updated", _httpContext.Session.GetString("success"));
        }

        [Fact]
        public void Update_ReturnsRedirectToAction_WhenModelStateIsInvalid()
        {
            // Arrange
            var studentSubject = new StudentSubject { Id = 1 };
            _controller.ModelState.AddModelError("error", "Invalid data");

            // Act
            var result = _controller.Update(studentSubject) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Faculty", result.ActionName);
            Assert.Equal("error", _httpContext.Session.GetString("error"));
        }
    }
}
