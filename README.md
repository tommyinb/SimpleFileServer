# SimpleFileServer is a Simple Web Server.
![preview](https://raw.githubusercontent.com/tommyinb/SimpleFileServer/master/SimpleFileServer/bin/preview.png)
* One Single Executable: [SimpleFileServer.exe](https://raw.githubusercontent.com/tommyinb/SimpleFileServer/master/SimpleFileServer/bin/SimpleFileServer.exe)
* Size: 1.1MB
* Framework: .NET 4.5

## Simple Usage
It is designed to let web clients use GET/POST/DELETE requests to access files. eg.
* [POST] http://localhost:8892/example1.png
* [GET] http://localhost:8892/example2.txt
* [DELETE] http://localhost:8892/example3.jpg

### Turn on server
1. Double click "[SimpleFileServer.exe](https://raw.githubusercontent.com/tommyinb/SimpleFileServer/master/SimpleFileServer/bin/SimpleFileServer.exe)".
2. Done.

### GET a file
1. Put a file in the program directory, eg. put "example.txt".
2. Open any browser.
3. Goto "http://localhost:8892/example.txt".
4. You get your file.

### POST a file
1. Open any HTTP tools, eg. Postman.
2. Upload your file to url, eg. "http://localhost:8892/example.txt".
3. Check your file and it will be in the program directory.
4. Design your own upload form.

### DELETE a file
1. Open any HTTP tools, eg. Postman.
2. Send a DELETE action to url, eg. "http://localhost:8892/example.txt".
3. File is deleted.
4. You can also delete through any  browser.
5. Browse file url with "?delete" parameter, eg. "http://localhost:8892/example2.txt?delete".
6. File is deleted.

### List a folder
1. Just browse folder url, eg. "http://localhost:8892/folder1".
2. Files are listed.
3. Adding "?folders" or "?both" parameter lists sub-folders as well.

### Change server port
1. Start program with parameter, eg. "SimpleFileServer.exe 8888".
2. Browser "http://localhost:8888" to check the new port.

### Change file directory
1. Start program with second parameter as directory, eg. "SimpleFileServer.exe 8888 folder1".
2. Add files into "folder1" folder, eg. add a "example.txt".
2. Browser "http://localhost:8888/example.txt" to check the new directory.

## Simple Customization
    Responses = new List<IServerResponse>();
    Responses.Add(new DeleteFileResponse(directory));
    Responses.Add(new GetFileResponse(directory));
    Responses.Add(new PostFileResponse(directory));
    Responses.Add(new IndexResponse(directory));
    Responses.Add(new DirectoryResponse(directory));
    Responses.Add(new BadRequestResponse());

Just add your own code to the collection.

    public interface IServerResponse
    {
        bool IsValid(HttpListenerRequest request);
        Task Response(HttpListenerContext context);
    }

## Simple Idea
Simple is the key. Its core bases on only 4 short blog posts - less than 50 lines.

Anyone can read it within 5 minutes.
* http://bookmi.blogspot.hk/2015/01/cweb-server.html
* http://bookmi.blogspot.hk/2015/01/c-web-server-part-2.html
* http://bookmi.blogspot.hk/2015/01/c-web-server-part-3.html
* http://bookmi.blogspot.hk/2015/01/c-web-server-part-4.html

## Download
* One Single Executable: [SimpleFileServer.exe](https://raw.githubusercontent.com/tommyinb/SimpleFileServer/master/SimpleFileServer/bin/SimpleFileServer.exe)
* Size: 1.1MB
* Framework: .NET 4.5
* Release Date: 2016/04/18
