using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mumblr.Models
{
    public interface IMumblrRepository
    {
        void AddFriend(string userId, string friendId);
        IEnumerable<FriendSystem> GetAllFriends();
        void RemoveFriend(string userId, string friendId);
        void addPost(Post post);
        IEnumerable<Post> getFeed(string userId);
        IEnumerable<Post> getUserPosts(string userId);
        List<string> getUserFriends(string userId);
        void RemoveFriendSystemEntriesOfUser(string id);
        bool deleteUserPost(int id);
        Post getPost(int id);
        IEnumerable<string> getPostLikes(int Id);
        bool likePost(int id, string userId);
        bool unLikePost(int id, string userId);
        IEnumerable<Messages> getChat(string userId, string friendId);
        bool addChatMessage(string userId, string friendId, string message);
        bool removeMessagesOfDeletedProfiles(string userId);
        bool deleteUserMessage(int messageId);
        string returnMessageRecipient(int messageId);
        bool markMessagesAsRead(string recipient, string sender);
        int unreadMesssagesAmount(string recipient, string sender);
        int combinedUnreadMessages(string recipient);
        bool verifyItIsUsersOwnMessage(int id, string userId);
        bool verifyItIsUsersOwnPost(int id, string userId);
    }
}
