This is a quick, easy to use wrapper for qBittorrent located at:
https://github.com/Conan1996-2/qBittorrent-WebUI-API


The Code structure follows the API tree, so as example,
"yourvariable'.api.V2.auth is the /api/v2/auth/methodName in https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-4.1)#authentication


Sample code:
Dim Torrentconnect As TorrentControl = New TorrentControl
Torrentconnect.api.v2.auth.Login("http://192.168.1.51", "80", "username", "userpassword")
Dim d As String = Torrentconnect.api.v2.torrents.AddNewTorrent(myMagnetString)


Once the user logs in, the login information is stored in the class.

This has not been fully tested. I use it and it does work for what I use it for.
