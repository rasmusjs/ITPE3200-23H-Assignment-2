using System;
using System.Collections.Generic;
using forum.Models;

namespace forum.ViewModels
{
    public class TopicListViewModel
    {
        public IEnumerable<Topic> Topics;

        public TopicListViewModel(IEnumerable<Topic> topics)
        {
            Topics = topics;
        }
    }
}