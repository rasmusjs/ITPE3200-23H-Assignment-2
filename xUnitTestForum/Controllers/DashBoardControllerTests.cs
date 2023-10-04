using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using forum.Controllers;
using forum.DAL;
using forum.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace xUnitTestForum.Controllers;

public class DashBoardControllerTests : Controller
{
    [Fact]
    public async Task TestDashBoard()
    {
        //https://stackoverflow.com/questions/29485285/can-not-find-user-identity-getuserid-method
        //return User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Arrange

        var userList = new List<ApplicationUser>()
        {
            new()
            {
                UserName = "CoolGuy",
                PasswordHash = "AQAAAAEAACcQAA",
                CreationDate = DateTime.Now
            },
            new()
            {
                UserName = "HackerMan",
                PasswordHash = "AQAAAAEAACcQAA",
                CreationDate = DateTime.Now
            },
            new()
            {
                UserName = "TheBoss",
                PasswordHash = "AQAAAAEAACcQAA",
                CreationDate = DateTime.Now
            }
        };

        var postsList = new List<Post>()
        {
            new()
            {
                Title = "\ud83d\udcdc Why JavaScript should be considered a gift from GOD! \ud83d\udcdc",
                Content =
                    "Ladies and gentlemen, gather 'round, for today, we embark on a divine journey through the ethereal realms of JavaScript! \ud83d\ude80\ud83c\udf0c\n\n##  **Unleash the Versatility**\nBehold, for JavaScript is the omnipotent chameleon of coding languages! It dances seamlessly not only in the sacred halls of browsers but also dons the crown of servers (praise be to Node.js) and blesses mobile apps with its touch (hail React Native)!\n\n##  **A Cosmic Force of Popularity**\nIt is not just a language; it's a celestial phenomenon! JavaScript's ubiquity transcends the boundaries of realms, making it one of the most widely-used languages, embraced by mortals and tech gods alike.\n\n##  **A Sacred Evolution**\nJavaScript is on an eternal quest for perfection. ES6, ES7, ES8... it evolves faster than the speed of light, adapting to the celestial needs of modern development.\n\n##  **The Art of Interactivity**\nWitness the magic as JavaScript breathes life into the lifeless! It grants websites the gift of interactivity and dynamism, ensnaring users in a spellbinding trance.\n\n##  **A Cosmic Job Market**\nBy embracing the holy scriptures of JavaScript, you open the gates to an abundance of job opportunities in the ever-expanding tech universe. Devs, rejoice! \ud83d\ude4c\ud83c\udf20\n\n##  **The Fellowship of Community**\nJavaScript's community is not just a community; it's a sacred brotherhood! On StackOverflow, GitHub, and countless other altars, the faithful gather to bestow wisdom upon the seeking souls.\n\nYes, it has its quirks (the enigmatic \"undefined\" and the mystical \"NaN\"), but what godly creation doesn't have its mysteries? \ud83e\udd37\u200d\u2642\ufe0f\ud83c\udf0c\n\nSo, let us kneel before JavaScript, the divine thread that weaves the very fabric of the web, a celestial gift that keeps on giving to us humble developers! \ud83d\ude4f\n\nDo you too believe in the divinity of JavaScript or have celestial tales to share? \ud83c\udf20\ud83d\udd2e #JavaScriptGift #DevotionToCode\n",
                DateCreated = DateTime.Now,
                DateLastEdited = DateTime.Now,
                UserId = userList[0].Id,
                CategoryId = 1,
                Tags = new List<Tag>
                {
                    new()
                    {
                        Name = "JavaScript"
                    },
                    new()
                    {
                        Name = "Science"
                    }
                }
            },
            new()
            {
                Title = "Second post",
                Content = "This is the second post",
                DateCreated = DateTime.Now,
                DateLastEdited = DateTime.Now,
                UserId = userList[1].Id,
                CategoryId = 2,
                Tags = new List<Tag>()
                {
                    new()
                    {
                        Name = "PowerShell"
                    },
                    new()
                    {
                        Name = "Windows"
                    }
                }
            },
            new()
            {
                Title = "Third post",
                Content = "This is the third post",
                DateCreated = DateTime.Now,
                DateLastEdited = DateTime.Now,
                UserId = userList[2].Id,
                CategoryId = 3,
                Tags = new List<Tag>
                {
                    new()
                    {
                        Name = "Java"
                    },
                }
            }
        };


        var mockRepo = new Mock<IForumRepository<ApplicationUser>>();
        mockRepo.Setup(repo => repo.GetUserActivity("1"))
            .ReturnsAsync(userList[0]);
        var mockLogger = new Mock<ILogger<ApplicationUser>>();
        var controller = new DashBoardController(mockRepo.Object, mockLogger.Object);

        // Act
        var result = await controller.Dashboard();

        // Assert
        /*var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<ApplicationUser>>(viewResult.ViewData.Model);
        Assert.Equal(postsList.Count, model.Count());
        Assert.Equal(postsList[0].Title, model.ElementAt(0).Title);*/
    }
}