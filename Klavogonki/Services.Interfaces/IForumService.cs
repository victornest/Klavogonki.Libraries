using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IForumService : IProgressNotifier
    {
        Task<ForumPost> GetForumPost(string messageUrl);
        string EditForumPost(ForumPost post, string message);
        Task<List<Topic>> GetForumTopics(string forumName, int maxPages = int.MaxValue, string nameSearchPattern = null);
        Task<List<Event>> GetLastEventsByNick(string author, string nameSearchPattern = null);
        string PostNewMessage(string topicId, string topicUrl, string kgToken, string message);
        void WriteTopicsToFile(List<Topic> topics, string forumName);
    }
}