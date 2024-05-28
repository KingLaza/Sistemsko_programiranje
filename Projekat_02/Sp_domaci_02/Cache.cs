using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat_02
{
    public static class Cache
    {
        private static readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<string, string> cache = new Dictionary<string, string>();

        public static string ReadFromCache(string key)
        {
            cacheLock.EnterReadLock();
            try
            {
                if (cache.TryGetValue(key, out string value))
                {
                    return value;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                throw;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }
        public static void WriteToCache(string key, string value)
        {
            cacheLock.EnterWriteLock();
            try
            {
                cache[key] = value;
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                throw;
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }
        public static void RemoveFromCache(string key)
        {
            cacheLock.EnterWriteLock();
            try
            {
                //sta ako kljuc ne postoji?
                if (cache.ContainsKey(key))
                    cache.Remove(key);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

    }
}
