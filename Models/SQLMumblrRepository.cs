using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mumblr.Models
{
    public class SQLMumblrRepository : IMumblrRepository
    {
        private readonly AppDbContext context;

        public SQLMumblrRepository(AppDbContext context)
        {
            this.context = context;
        }


        public void AddFriend(string userId, string friendId)
        {
            context.FriendSystem.Add(new FriendSystem
            {
                UserId = userId,
                FriendId = friendId
                
                
            });


            context.SaveChanges();
        }

        public void addPost(Post post)
        {
            context.Posts.Add(post);
            context.SaveChanges();
        }

        public IEnumerable<FriendSystem> GetAllFriends()
        {
            return context.FriendSystem;
        }



        public IEnumerable<Post> getFeed(string userId)
        {

            var userIds = getUserFriends(userId);

            userIds.Add(userId);

            List<Post> feed = new List<Post>();

            foreach (var x in userIds)
            {
                var userPosts = getUserPosts(x);

                foreach (var y in userPosts)
                {
                    feed.Add(y);
                }
            }

            feed.Sort((x, y) => DateTime.Compare(x.Date, y.Date));

            feed.Reverse();

            return feed;

        }

        // returns a list of UserNames that the specified user is friends with
        public List<string> getUserFriends(string userId)
        {
            // Get the rows in which the userId occurs
            var friendSystemEntries = context.FriendSystem.Where(x => x.UserId == userId || x.FriendId == userId).ToList();

            // List containing only the friendIds
            List<string> userFriends = new List<string>();

            // Extracting single values from tablerows
            foreach (var x in friendSystemEntries)
            {
                if (x.FriendId != userId)
                {
                    userFriends.Add(x.FriendId);
                }
                else if (x.UserId != userId)
                {
                    userFriends.Add(x.UserId);
                }
            }
            return userFriends;
        }

        public IEnumerable<Post> getUserPosts(string userId)
        {
            var userPosts = context.Posts.Where(x => x.UserId == userId);

            return userPosts;

        }

        public void RemoveFriend(string userId, string friendId)
        {

            var entryToDelete = context.FriendSystem.First(a => a.FriendId == friendId && a.UserId == userId || a.FriendId == userId && a.UserId == friendId);
            context.FriendSystem.Remove(entryToDelete);
            context.SaveChanges();
        }

        public bool deleteUserPost(int id)
        {
            bool success = false;
            try
            {
                var entryToDelete = context.Posts.First(x => x.PostId == id);
                context.Posts.Remove(entryToDelete);
                context.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                success = false;
                
            }
            return success;
        }

        public bool deleteUserMessage(int id)
        {
            bool success = false;
            try
            {
                var entryToDelete = context.Chat.First(x => x.MessagesId == id);
                context.Chat.Remove(entryToDelete);
                context.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                success = false;

            }
            return success;
        }

        public Post getPost(int id)
        {
            try
            {
                var post = context.Posts.First(x => x.PostId == id);
                return post;
            }catch(Exception e)
            {
                return null;
            }
            
        }

        // Returns a List of Users who liked a Post with a given Id
        public IEnumerable<string> getPostLikes(int Id)
        {
            var getPost = context.PostLikes.Where(a => a.Post.PostId == Id);

            List<string> usersWhoLikedThisPost = new List<string>();

            foreach(var post in getPost)
            {
                usersWhoLikedThisPost.Add(post.UserId);
            }

            return usersWhoLikedThisPost;
        }

        // Takes an Id of a Post and a UserId and adds that entry to Postlikes
        public bool likePost(int id, string userId)
        {
            var success = false;
            try
            {
                var newLike = new PostLikes
                {
                    UserId = userId,
                    Post = getPost(id)
                };
                context.PostLikes.Add(newLike);
                context.SaveChanges();
                success = true;
            } catch(Exception e)
            {
                
            }
            return success;
        }

        public bool unLikePost(int id, string userId)
        {
            var success = false;
            try
            {
                var entryToDelete = context.PostLikes.First(a => a.Post.PostId == id && a.UserId == userId);
                context.PostLikes.Remove(entryToDelete);
                context.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {

            }
            return success;
        }

        public IEnumerable<Messages> getChat(string userId, string friendId)
        {
            var messages = context.Chat.Where(a => a.recipient == userId && a.sender == friendId || a.recipient == friendId && a.sender == userId);

            List<Messages> chat = new List<Messages>();

            foreach(var message in messages)
            {
                chat.Add(message);
            }

            chat.Sort((x, y) => DateTime.Compare(x.Date, y.Date));

            chat.Reverse();

            return chat;
        }

        public bool addChatMessage(string userId, string friendId, string message)
        {
            var newMessage = new Messages()
            {
                Message = message,
                sender = userId,
                recipient = friendId,
                Date = DateTime.Now
            };

            var success = false;
            try
            {
                context.Chat.Add(newMessage);
                context.SaveChanges();
                success = true;
            }catch(Exception e)
            {

            }
            return success;
        }

        public bool removeMessagesOfDeletedProfiles(string userId)
        {
            try
            {
                var entriesToDelete = context.Chat.Where(a => a.recipient == userId || a.sender == userId);

                foreach (var entry in entriesToDelete)
                {
                    context.Chat.Remove(entry);
                }
                context.SaveChanges();
                return true;
            }
            catch(Exception e)
            {
                return false;
            }            
        }

        public void RemoveFriendSystemEntriesOfUser(string userId)
        {
            var entriesToDelete = context.FriendSystem.Where(a => a.FriendId == userId || a.UserId == userId);

            foreach (var entry in entriesToDelete)
            {
                context.FriendSystem.Remove(entry);
            }

            context.SaveChanges();
        }

        public string returnMessageRecipient(int messageId)
        {
            var recipient = context.Chat.Where(a => a.MessagesId == messageId).Select(a => a.recipient).Single();
           

            return recipient;
        }

        public bool markMessagesAsRead(string recipient, string sender)
        {
            bool success = false;
            try
            {
                var readMessages = context.Chat.Where(a => a.recipient == recipient && a.sender == sender);
                foreach(var message in readMessages)
                {
                    message.isRead = true;
                }
                context.SaveChanges();
                success = true;
            }
            catch(Exception e)
            {

            }
            return success;
        }

        public int unreadMesssagesAmount(string recipient, string sender)
        {
            int unreadMessages = 0;
            var messages = context.Chat.Where(a => a.recipient == recipient && a.sender == sender);
            foreach(var message in messages)
            {
                if(!message.isRead)
                {
                    unreadMessages += 1;
                }
            }
            return unreadMessages;
        }

        public int combinedUnreadMessages(string recipient)
        {
            int unreadMessages = 0;
            var messages = context.Chat.Where(a => a.recipient == recipient);
            foreach (var message in messages)
            {
                if (!message.isRead)
                {
                    unreadMessages += 1;
                }
            }
            return unreadMessages;
        }

        public bool verifyItIsUsersOwnMessage(int id, string userId)
        {
            var entry = context.Chat.Where(a => a.MessagesId == id && a.sender == userId);
            if(entry.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool verifyItIsUsersOwnPost(int id, string userId)
        {
            var entry = context.Posts.Where(a => a.PostId == id && a.UserId == userId);
            if (entry.Any())
            {
                return true;
            }
            else
            {
                return false;
            };
        }
    }
}

