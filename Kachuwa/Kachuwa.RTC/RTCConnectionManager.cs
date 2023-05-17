using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Kachuwa.Web;
using Kachuwa.Identity.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Kachuwa.RTC
{
    public class MemoryConnectionManager: IRTCConnectionManager
    {
        public ConcurrentDictionary<string, RTCUser> RealWebUsers { get; set; } = new ConcurrentDictionary<string, RTCUser>();

        public RTCUser GetWebUserById(string connectionId)
        {
            return RealWebUsers.FirstOrDefault(p => p.Key == connectionId).Value;
        }
      

        public ConcurrentDictionary<string, RTCUser> GetAll()
        {
            return RealWebUsers;
        }

        public string GetId(RTCUser user)
        {
            return RealWebUsers.FirstOrDefault(p => p.Value == user).Key;
        }

        private bool CheckExistingConnectionUser(string connectionId)
        {
            var context = ContextResolver.Context;
            //does'nt exist

            //for logged in user
            if (context.User.Identity.IsAuthenticated)
            {
                string userId = context.User.Identity.GetIdentityUserId().ToString();
                if (!RealWebUsers.ContainsKey(userId))
                {

                    //session exist
                    var existingUser = RealWebUsers.FirstOrDefault(p => p.Key == userId);
                    if (existingUser.Key != null)
                    {
                        //adding new connectionid
                        existingUser.Value.ConnectionIds.Add(connectionId);
                        return true;
                    }
                    else
                    {
                        // if session does not exist add
                        return false;
                    }
                }
            }
            else
            {//guest user tracking from sessions
                string userId = context.Session.Id;
                if (!RealWebUsers.ContainsKey(userId))
                {

                    //session exist
                    var existingUser = RealWebUsers.FirstOrDefault(p => p.Key == userId);
                    if (existingUser.Key != null)
                    {
                        //adding new connectionid
                        existingUser.Value.ConnectionIds.Add(connectionId);
                        return true;
                    }
                    else
                    {
                        // if session does not exist add
                        return false;
                    }
                }
            }
            return false;
        }
        public bool AddUser(string connectionId)
        {
            //string connectionId = CreateConnectionId();
            if (!CheckExistingConnectionUser(connectionId))
            {
                var rtcUser = new RTCUser();
                var context = ContextResolver.Context;
                string ua = context.Request.Headers["User-Agent"].ToString();
                rtcUser.UserDevice = ua;//new UserAgent(ua).Browser.ToString();
                rtcUser.ConnectionIds.Add(connectionId);
                rtcUser.IdentityUserId = context.User.Identity.GetIdentityUserId();
                rtcUser.SessionId = context.Session.Id;
                RealWebUsers.TryAdd(
                    context.User.Identity.IsAuthenticated
                        ? context.User.Identity.GetIdentityUserId().ToString()
                        : context.Session.Id, rtcUser);
                return true;
            }
            return false;
        }

        public bool AddUser(RTCUser user)
        {
            throw new NotImplementedException();
        }

        public bool RemoveUser(string connectionId)
        {
            RTCUser user;
            var context = ContextResolver.Context;
            RealWebUsers.TryGetValue(context.User.Identity.IsAuthenticated
                ? context.User.Identity.GetIdentityUserId().ToString()
                : context.Session.Id, out user);
            if (user != null)
            {
                user.ConnectionIds.Remove(connectionId);
                if (user.ConnectionIds.Count == 0)
                {
                    RealWebUsers.TryRemove(context.User.Identity.IsAuthenticated
                        ? context.User.Identity.GetIdentityUserId().ToString()
                        : context.Session.Id, out user);
                }
                return true;
            }


            return false;
        }

        public Task UpdateGroupName(string groupName, long identityUserId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateGroupName(string groupName, string connectionId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetUserConnectionIds(long identityUserId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetOnlineUserByHub(string hubName)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetOnlineUser()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetUserConnectionIdsByRoles(string role)
        {
            throw new NotImplementedException();
        }

        public Task<RtcUserStatus> GetOnlineUserStatus()
        {
            throw new NotImplementedException();
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
