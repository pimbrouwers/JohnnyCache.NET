# Johnny Cache
[![Build Status](https://travis-ci.org/pimbrouwers/johnny-cache.svg?branch=master)](https://travis-ci.org/pimbrouwers/johnny-cache/)

A thread-safe caching abstraction for .NET -- leverages object cache, filesystem and Azure Blob Storage (Amazon S3 coming soon). Johnny Cache was designed to improve application performance in both single-server and web-farm systems.

## Setup/Configuration
There are 3 primary configuration variables that need to be set in your ``App.Config/Web.Config``
```xml
    <!-- JOHNNY CACHE -->
    <add key="JC-ObjectCacheName" value="JohnnyCache-SomeProject"/>
    <add key="JC-WorkingDirectory" value="c:\temp\JohnnyCache\SomeProject"/>
    <add key="JC-ExpirationInSeconds" value="300"/>
```
Note: None of the above configuration variables are required, as they all are backed by default return values;

## Azure Configuration
To leverage Azure Blob Storage add the following keys to your ``App.Config/Web.Config``
```xml
    <add key="JC-AzureAccountName" value="{{YOUR ACCOUNT NAME}}"/>
    <add key="JC-AzureAccountKey" value="{{YOUR ACCOUNT KEY}}"/>
```

## Amazon S3 Configuration
```c#
    throw new NotImplementedException("Coming Soon!");
```

## Usage
The API for Johnny Cache is very simple, containing only 3 methods: ``Get()`` ``Set()`` ``Delete()``.
### Get
The ``Get()`` method will:
- Check Object Cache
- Check the File Dependancy
- Check External Providers (if configured, see above)

The primary method parameter here is the ``key``. For "stale" checks you can optionally pass a ``cacheDurationSeconds`` param to override the library-governing expiration.
```c#
string cacheKey = "SomeUniqueCacheKey";
YourCoolObject mycoolObject = JohnnyCache.Get<YourCoolObject>(cacheKey) as YourCoolObject; //using library expiration
YourCoolObject mycoolObject = JohnnyCache.Get<YourCoolObject>(cacheKey, 6000) as YourCoolObject; //overriding library expiration
```
### Set
The ``Set()`` method will:
- Write to Object Cache
- Write to the File Dependancy
- Upload to External Providers (if configured, see above)

The primary method parameter here is the ``key``. For "stale" checks you can optionally pass a ``cacheDurationSeconds`` param to override the library-governing expiration.
```c#
string cacheKey = "SomeUniqueCacheKey";
JohnnyCache.Set(myCoolObject, cacheKey); //using library expiration
JohnnyCache.Set(myCoolObject, cacheKey, 6000); //overriding library expiration
```
### Delete
The delete method will purge the object(s) from all levels of cache. 

The lone method parameter here is the ``key``.
```c#
string cacheKey = "SomeUniqueCacheKey";
JohnnyCache.Set(cacheKey);
```
