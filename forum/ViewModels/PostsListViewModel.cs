using System;
using System.Collections.Generic;
using forum.Models;

namespace forum.ViewModels
{
    public class PostsListViewModel
    {
        public IEnumerable<Post> Posts;
        public string? CurrentViewName;

        public PostsListViewModel(IEnumerable<Post> posts, string? currentViewName)
        {
            Posts = posts;
            CurrentViewName = currentViewName;
        }
    }
}