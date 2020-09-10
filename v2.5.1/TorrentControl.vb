Imports System.Net
Imports System.Reflection

'https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-(qBittorrent-4.1)#login
' API
Public Class TorrentControl
    Public api As apiClass
    Private WebUIVersion As String = "2.5.1"

    Public Class CookieAwareWebClient
        Inherits WebClient

        Private cc As New CookieContainer()
        Private lastPage As String

        Protected Overrides Function GetWebRequest(ByVal address As System.Uri) As WebRequest
            Dim R = MyBase.GetWebRequest(address)
            If TypeOf R Is HttpWebRequest Then
                With DirectCast(R, HttpWebRequest)
                    .CookieContainer = cc
                    If Not lastPage Is Nothing Then .Referer = lastPage
                End With
            End If
            lastPage = address.ToString()
            Return R
        End Function
    End Class

    Public Class apiClass
        Public v2 As v2Class
        Protected Client As CookieAwareWebClient = New CookieAwareWebClient

        Public Class v2Class
            Protected Base As apiClass
            Public auth As authClass
            Public app As appClass
            Public sync As syncClass
            Public torrents As torrentsClass
            Public rss As RSSClass
            Public search As SearchClass

            Public Class authClass
                Friend Base As apiClass
                Private _connectionString As String = ""
                Private _siteAddress As String = ""
                Private _sitePort As String = ""
                Private _userName As String = ""
                Private _password As String = ""

#Region "Methods"

                Public Function ReLogin()
                    Return SendClientRequest("/api/v2/auth/login", {{"username", _userName}, {"password", _password}})
                End Function

                Public Function Login(Site As String, port As String, Username As String, Password As String)
                    _siteAddress = Site
                    _sitePort = ":" & port
                    _userName = Username
                    _password = Password
                    _connectionString = _siteAddress & _sitePort
                    Return ReLogin()
                End Function

                Public Function Logout()
                    Return SendClientRequest("/api/v2/auth/logout")
                End Function

                Public Function ClientAddress()
                    Return _connectionString
                End Function

                Friend Function SendClientRequest(Path As String, Optional ByRef Requests(,) As String = Nothing) As String
                    Dim Response As String = ""
                    Try
                        If Requests Is Nothing Then : Response = Base.Client.DownloadString(_connectionString & Path)
                        Else
                            Dim reqparm As New Specialized.NameValueCollection
                            For Index = Requests.GetLowerBound(0) To Requests.GetUpperBound(0)
                                If Requests(Index, Requests.GetUpperBound(1)) IsNot Nothing Then reqparm.Add(Requests(Index, Requests.GetLowerBound(1)), Requests(Index, Requests.GetUpperBound(1)))
                            Next
                            Dim responsebytes = Base.Client.UploadValues(_connectionString & Path, "POST", reqparm)
                            Response = (New Text.UTF8Encoding).GetString(responsebytes)
                        End If
                    Catch ex As WebException
                    End Try
                    Dim responseField As FieldInfo = Base.Client.GetType().GetField("m_WebResponse", BindingFlags.Instance Or BindingFlags.NonPublic)
                    Return Response
                End Function

                Public Function ParseJSON(Value As String) As Newtonsoft.Json.Linq.JObject
                    Dim Data As String = "{response:" & If(Value.Length > 0, Value, """""") & "}"
                    Return Newtonsoft.Json.Linq.JObject.Parse(Data)
                End Function

#End Region
            End Class

            Public Class appClass
                Friend Base As apiClass
                Private connectionPath As String = "/api/v2/app/"

#Region "Constants"

                Public Class scan_dirs
                    Public Const Download_to_monitored_folder As Integer = 0
                    Public Const Download_to_default_save_path As Integer = 1
                End Class

                Public Class scheduler_days
                    Public Const Every_day As Integer = 0
                    Public Const Every_weekday As Integer = 1
                    Public Const Every_weekend As Integer = 2
                    Public Const Every_monday As Integer = 3
                    Public Const Every_tuesday As Integer = 4
                    Public Const Every_wednesday As Integer = 5
                    Public Const Every_thursday As Integer = 6
                    Public Const Every_friday As Integer = 7
                    Public Const Every_saturday As Integer = 8
                    Public Const Every_sunday As Integer = 9
                End Class

                Public Class encryption
                    Public Const Prefer_encryption As Integer = 0
                    Public Const Force_encryption_on As Integer = 1
                    Public Const Force_encryption_off As Integer = 2
                End Class

                Public Class proxy_type
                    Public Const Proxy_disabled As Integer = -1
                    Public Const HTTP_without_authentication As Integer = 1
                    Public Const SOCKS5_without_authentication As Integer = 2
                    Public Const HTTP_with_authentication As Integer = 3
                    Public Const SOCKS5_with_authentication As Integer = 4
                    Public Const SOCKS4_with_authentication As Integer = 5
                End Class

                Public Class dyndns_service
                    Public Const Use_DyDNS As Integer = 0
                    Public Const Use_NOIP As Integer = 1
                End Class

                Public Class max_ratio_act
                    Public Const Pause_torrent As Integer = 0
                    Public Const Remove_torrent As Integer = 1
                End Class

                Public Class bittorrent_protocol
                    Public Const TCP_and_uTP As Integer = 0
                    Public Const TCP As Integer = 1
                    Public Const uTP As Integer = 2
                End Class

                Public Class upload_choking_algorithm
                    Public Const Round_Robin As Integer = 0
                    Public Const Fastest_upload As Integer = 1
                    Public Const Anti_leech As Integer = 2
                End Class

                Public Class upload_slots_behavior
                    Public Const Fixed_slots As Integer = 0
                    Public Const Upload_rate_based As Integer = 1
                End Class

                Public Class utp_tcp_mixed_mode
                    Public Const Prefer_TCP As Integer = 0
                    Public Const Peer_proportional As Integer = 1
                End Class

#End Region

#Region "Methods"

                Public Function GetAppVersion() As String
                    Return Base.SendClientRequest(connectionPath & "version")
                End Function

                Public Function GetAPIVersion() As String
                    Return Base.SendClientRequest(connectionPath & "webapiVersion")
                End Function

                Public Function GetBuildInfo() As String
                    Return Base.SendClientRequest(connectionPath & "buildInfo")
                End Function

                Public Function ShutdownApplication() As String
                    Return Base.SendClientRequest(connectionPath & "shutdown")
                End Function

                Public Function GetApplicationPreferences() As String
                    Return Base.SendClientRequest(connectionPath & "preferences")
                End Function

                Public Function SetApplicationPreferences(newPreferences As String) As String
                    Return Base.SendClientRequest(connectionPath & "setPreferences")
                End Function

                Public Function GetDefaultSavePath() As String
                    Return Base.SendClientRequest(connectionPath & "defaultSavePath")
                End Function

#End Region
            End Class

            Public Class syncClass
                Friend Base As apiClass
                Private connectionPath As String = "/api/v2/sync"

#Region "Methods"

                Public Function GetMainData(Optional ID As String = "0") As String
                    Return Base.SendClientRequest(connectionPath & "/maindata?rid=" & ID)
                End Function

                Public Function GetMainData(Hash As String, Optional ID As String = "0") As String
                    Return Base.SendClientRequest(connectionPath & "/torrentPeers?rid=" & ID & "&hash=" & Hash)
                End Function

#End Region
            End Class

            Public Class transfer
                Friend Base As apiClass
                Private connectionPath As String = "/api/v2/transfer"

#Region "Constants"

                Public Class connection_status
                    Public Const connected As String = "connected"
                    Public Const firewalled As String = "firewalled"
                    Public Const disconnected As String = "disconnected"
                End Class

#End Region

#Region "Methods"

                Public Function GetTransferInformation()
                    Return Base.SendClientRequest(connectionPath & "/info")
                End Function

                Public Function GetAlternativeSpeedLimit()
                    Return Base.SendClientRequest(connectionPath & "/speedLimitsMode")
                End Function

                Public Function ToggleAlternativeSpeedLimit()
                    Return Base.SendClientRequest(connectionPath & "/toggleSpeedLimitsMode")
                End Function

                Public Function GetGlobalDownloadLimit()
                    Return Base.SendClientRequest(connectionPath & "/downloadLimit")
                End Function

                Public Function SetGlobalDownloadLimit(Limit As String)
                    Return Base.SendClientRequest(connectionPath & "/setDownloadLimit?limit=" & Limit)
                End Function

                Public Function GetUploadLimit()
                    Return Base.SendClientRequest(connectionPath & "/uploadLimit")
                End Function

                Public Function SetUploadLimit(Limit As String)
                    Return Base.SendClientRequest(connectionPath & "/setUploadLimit?limit=" & Limit)
                End Function

                Public Function BanPeers(Peers As String)
                    Return Base.SendClientRequest(connectionPath & "/banPeers?peers=" & Peers)
                End Function

#End Region
            End Class

            Public Class torrentsClass
                Friend Base As apiClass
                Private connectionPath As String = "/api/v2/torrents/"

#Region "Constants"

                Public Class Filters
                    Public Const all As String = "all"
                    Public Const downloading As String = "downloading"
                    Public Const completed As String = "completed"
                    Public Const paused As String = "paused"
                    Public Const active As String = "active"
                    Public Const inactive As String = "inactive"
                    Public Const resumed As String = "resumed"
                    Public Const stalled As String = "stalled"
                    Public Const stalled_uploading As String = "stalled_uploading"
                    Public Const stalled_downloading As String = "stalled_downloading"
                End Class

                Public Class states
                    Public Const error_ As String = "error"
                    Public Const missingFiles As String = "missingFiles"
                    Public Const uploading As String = "uploading"
                    Public Const pausedUP As String = "pausedUP"
                    Public Const queuedUP As String = "queuedUP"
                    Public Const stalledUP As String = "stalledUP"
                    Public Const checkingUP As String = "checkingUP"
                    Public Const forcedUP As String = "forcedUP"
                    Public Const allocating As String = "allocating"
                    Public Const downloading As String = "downloading"
                    Public Const metaDL As String = "metaDL"
                    Public Const pausedDL As String = "pausedDL"
                    Public Const queuedDL As String = "queuedDL"
                    Public Const stalledDL As String = "stalledDL"
                    Public Const checkingDL As String = "checkingDL"
                    Public Const forceDL As String = "forceDL"
                    Public Const checkingResumeData As String = "checkingResumeData"
                    Public Const moving As String = "moving"
                    Public Const unknown As String = "unknown"
                End Class

#End Region

#Region "Methods"

                Public Function GetTorrentList(Optional filter As String = Nothing, Optional Category As String = Nothing, Optional sort As String = Nothing, Optional reverse As String = "false", Optional limit As String = Nothing, Optional offset As String = Nothing, Optional hashes As String = Nothing) As String
                    Dim StringBuilder As String = ""
                    If filter IsNot Nothing Then StringBuilder &= "filter=" & filter
                    If Category IsNot Nothing Then StringBuilder &= If(StringBuilder.Length() > 0, "&", "") & "category=" & Category
                    If sort IsNot Nothing Then StringBuilder &= If(StringBuilder.Length() > 0, "&", "") & "sort=" & sort
                    If reverse IsNot Nothing Then StringBuilder &= If(StringBuilder.Length() > 0, "&", "") & "reverse=" & reverse
                    If limit IsNot Nothing Then StringBuilder &= If(StringBuilder.Length() > 0, "&", "") & "limit=" & limit
                    If offset IsNot Nothing Then StringBuilder &= If(StringBuilder.Length() > 0, "&", "") & "offset=" & offset
                    If hashes IsNot Nothing Then StringBuilder &= If(StringBuilder.Length() > 0, "&", "") & "hashes=" & hashes
                    Return Base.SendClientRequest(connectionPath & "info" & If(StringBuilder.Length > 0, "?" & StringBuilder, ""))
                End Function

                Public Function GetTorrentGenericProperties(Hash As String) As String
                    Return Base.SendClientRequest(connectionPath & "properties?hash=" & Hash)
                End Function

                Public Function GetTorrentTrackers(Hash As String) As String
                    Return Base.SendClientRequest(connectionPath & "trackers?hash=" & Hash)
                End Function

                Public Function getTorrentWebSeeds(Hash As String) As String
                    Return Base.SendClientRequest(connectionPath & "webseeds?hash=" & Hash)
                End Function

                Public Function GetTorrentContents(Hash As String) As String
                    Return Base.SendClientRequest(connectionPath & "files?hash=" & Hash)
                End Function

                Public Function GetTorrentPiecesStates(Hash As String) As String
                    Return Base.SendClientRequest(connectionPath & "pieceStates?hash=" & Hash)
                End Function

                Public Function GetTorrentPiecesHashes(Hash As String) As String
                    Return Base.SendClientRequest(connectionPath & "pieceHashes?hash=" & Hash)
                End Function

                Public Function PauseTorrents(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "pause?hashes=" & Hashes)
                End Function

                Public Function ResumeTorrents(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "resume?hashes=" & Hashes)
                End Function

                Public Function DeleteTorrents(Hashes As String, Optional DeleteFiles As String = "false") As String
                    Return Base.SendClientRequest(connectionPath & "delete?hashes=" & Hashes & "&deleteFiles=" & DeleteFiles)
                End Function

                Public Function RecheckTorrents(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "recheck?hashes=" & Hashes)
                End Function

                Public Function ReannounceTorrents(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "reannounce?hashes=" & Hashes)
                End Function

                Public Function AddNewTorrent(TorrentToAdd As String, Optional savePath As String = Nothing, Optional Category As String = Nothing, Optional skip_checking As String = "false",
                                              Optional paused As String = "false", Optional root_folder As String = Nothing, Optional rename As String = Nothing, Optional upLimit As String = Nothing,
                                              Optional dlLimit As String = Nothing, Optional autoTMM As String = Nothing, Optional sequentialDownload As String = "false", Optional firstLastPiecePrio As String = "false") As String
                    Return Base.SendClientRequest(connectionPath & "add",
                                                 {{"urls", TorrentToAdd}, {"savepath", savePath}, {"category", Category}, {"skip_checking", skip_checking}, {"paused", paused}, {"root_folder", root_folder},
                                                 {"rename", rename}, {"upLimit", upLimit}, {"dlLimit", upLimit}, {"autoTMM", autoTMM}, {"sequentialDownload", sequentialDownload}, {"firstLastPiecePrio", firstLastPiecePrio}})
                End Function

                Public Function AddTrackersToTorrent(Hash As String, Trackers As String) As String
                    Return Base.SendClientRequest(connectionPath & "addTrackers", {{"hash", Hash}, {"urls", Trackers.Replace("&", "%26")}})
                End Function

                Public Function EditTrackers(Hash As String, origUrl As String, newUrl As String) As String
                    Return Base.SendClientRequest(connectionPath & "editTracker?hash=" & Hash & "&origUrl=" & origUrl & "&newUrl=" & newUrl)
                End Function

                Public Function RemoveTrackers(Hash As String, Urls As String) As String
                    Return Base.SendClientRequest(connectionPath & "removeTrackers?hash=" & Hash & "&urls=" & Urls)
                End Function

                Public Function AddPeers(Hashes As String, Peers As String) As String
                    Return Base.SendClientRequest(connectionPath & "addPeers?hashes=" & Hashes & "&peers=" & Peers)
                End Function

                Public Function IncreaseTorrentPriority(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "increasePrio?hashes=" & Hashes)
                End Function

                Public Function DecreaseTorrentPriority(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "decreasePrio?hashes=" & Hashes)
                End Function

                Public Function MaximalTorrentPriority(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "topPrio?hashes=" & Hashes)
                End Function

                Public Function MinimalTorrentPriority(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "bottomPrio?hashes=" & Hashes)
                End Function

                Public Function SetFilePriority(Hash As String, Id As String, Priority As String) As String
                    Return Base.SendClientRequest(connectionPath & "filePrio?hash=" & Hash & "&id=" & Id & "&priority=" & Priority)
                End Function

                Public Function GetTorrentDownloadLimit(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "downloadLimit", {{"hashes", Hashes}})
                End Function

                Public Function SetTorrentDownloadLimit(Hashes As String, Limit As String) As String
                    Return Base.SendClientRequest(connectionPath & "setDownloadLimit", {{"hashes", Hashes}, {"limit", Limit}})
                End Function

                Public Function SetTorrentShareLimit(Hashes As String, RatioLimit As String, SeedingTimeLimit As String) As String
                    Return Base.SendClientRequest(connectionPath & "setShareLimits", {{"hashes", Hashes}, {"ratioLimit", RatioLimit}, {"seedingTimeLimit", SeedingTimeLimit}})
                End Function

                Public Function GetTorrentUploadLimit(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "uploadLimit", {{"hashes", Hashes}})
                End Function

                Public Function SetTorrentUploadLimit(Hashes As String, Limit As String) As String
                    Return Base.SendClientRequest(connectionPath & "setUploadLimit", {{"hashes", Hashes}, {"limit", Limit}})
                End Function

                Public Function SetTorrentLocation(Hashes As String, Location As String) As String
                    Return Base.SendClientRequest(connectionPath & "setLocation", {{"hashes", Hashes}, {"location", Location}})
                End Function

                Public Function SetTorrentName(Hash As String, Name As String) As String
                    Return Base.SendClientRequest(connectionPath & "rename", {{"hash", Hash}, {"name", Name}})
                End Function

                Public Function SetTorrentcategory(Hashes As String, Category As String) As String
                    Return Base.SendClientRequest(connectionPath & "setCategory", {{"hash", Hashes}, {"category", Category}})
                End Function

                Public Function GetAllCatagories() As String
                    Return Base.SendClientRequest(connectionPath & "catagories")
                End Function

                Public Function AddNewcategory(Category As String, SavePath As String) As String
                    Return Base.SendClientRequest(connectionPath & "createCategory", {{"category", Category}, {"savePath", SavePath}})
                End Function

                Public Function Editcategory(Category As String, SavePath As String) As String
                    Return Base.SendClientRequest(connectionPath & "editCategory", {{"category", Category}, {"savePath", SavePath}})
                End Function

                Public Function RemoveCatagories(Categories As String) As String
                    Return Base.SendClientRequest(connectionPath & "removeCategories", {{"categories", Categories}})
                End Function

                Public Function AddTorrentTags(Hashes As String, Tags As String) As String
                    Return Base.SendClientRequest(connectionPath & "addTags", {{"hashes", Hashes}, {"tags", Tags}})
                End Function

                Public Function RemoveTorrentTags(Hashes As String, Tags As String) As String
                    Return Base.SendClientRequest(connectionPath & "removeTags", {{"hashes", Hashes}, {"tags", Tags}})
                End Function

                Public Function GetAllTags() As String
                    Return Base.SendClientRequest(connectionPath & "tags")
                End Function

                Public Function CreateTags(Tags As String) As String
                    Return Base.SendClientRequest(connectionPath & "createTags", {{"tags", Tags}})
                End Function

                Public Function DeleteTags(Tags As String) As String
                    Return Base.SendClientRequest(connectionPath & "deleteTags", {{"tags", Tags}})
                End Function

                Public Function SetAutomaticTorrentManagement(Hashes As String, Optional Enable As String = "false") As String
                    Return Base.SendClientRequest(connectionPath & "setAutoManagement", {{"hashes", Hashes}, {"enable", Enable}})
                End Function

                Public Function ToggleSequentialDownload(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "toggleSequentialDownload?hashes=" & Hashes)
                End Function

                Public Function SetFirstLastPiecePriority(Hashes As String) As String
                    Return Base.SendClientRequest(connectionPath & "toggleFirstLastPiecePrio?hashes=" & Hashes)
                End Function

                Public Function SetForceStart(Hashes As String, Optional Value As String = "false") As String
                    Return Base.SendClientRequest(connectionPath & "setForceStart", {{"hashes", Hashes}, {"value", Value}})
                End Function

                Public Function SetSuperSeeding(Hashes As String, Optional Value As String = "false") As String
                    Return Base.SendClientRequest(connectionPath & "setSuperSeeding", {{"hashes", Hashes}, {"value", Value}})
                End Function

                Public Function RenameFile(Hash As String, Id As String, Name As String) As String
                    Return Base.SendClientRequest(connectionPath & "renameFile?hash=" & Hash & "&id=" & Id & "&name=" & Name)
                End Function

#End Region
            End Class

            Public Class RSSClass
                Friend Base As apiClass
                Private connectionPath As String = "/api/v2/rss/"

#Region "Methods"

                Public Function AddFolder(Path As String) As String
                    Return Base.SendClientRequest(connectionPath & "addFolder?path=" & Path)
                End Function

                Public Function AddFeed(Url As String, Optional Path As String = Nothing) As String
                    Dim StringBuilder As String = "url=" & Url
                    If Path IsNot Nothing Then StringBuilder &= "&path=" & Path
                    Return Base.SendClientRequest(connectionPath & "addFeed?path=" & StringBuilder)
                End Function

                Public Function RemoveItem(Path As String) As String
                    Return Base.SendClientRequest(connectionPath & "removeItem?path=" & Path)
                End Function

                Public Function MoveItem(ItemPath As String, DestPath As String) As String
                    Return Base.SendClientRequest(connectionPath & "moveItem?itemPath=" & ItemPath & "&destPath=" & DestPath)
                End Function

                Public Function GetAllItems(Optional withData As String = Nothing) As String
                    Dim StringBuilder As String = ""
                    If withData IsNot Nothing Then StringBuilder &= "?withData=" & withData
                    Return Base.SendClientRequest(connectionPath & "items" & StringBuilder)
                End Function

                Public Function MarkAsRead(itemPath As String, Optional articleId As String = Nothing) As String
                    Dim StringBuilder As String = "itemPath=" & itemPath
                    If articleId IsNot Nothing Then StringBuilder &= "&articleId=" & articleId
                    Return Base.SendClientRequest(connectionPath & "markAsRead?" & StringBuilder)
                End Function

                Public Function RefreshItem(itemPath As String) As String
                    Return Base.SendClientRequest(connectionPath & "refreshItem?itemPath=" & itemPath)
                End Function

                Public Function SetAutoDownloadingRule(RuleName As String, RuleDef As String) As String
                    Return Base.SendClientRequest(connectionPath & "setRule?ruleName=" & RuleName & "&ruleDef=" & RuleDef)
                End Function

                Public Function RenameAutoDownloadingRule(RuleName As String, NewRuleName As String) As String
                    Return Base.SendClientRequest(connectionPath & "renameRule?ruleName=" & RuleName & "&newRuleName=" & NewRuleName)
                End Function

                Public Function RemoveAutoDownloadingRule(RuleName As String) As String
                    Return Base.SendClientRequest(connectionPath & "removeRule?ruleName=" & RuleName)
                End Function

                Public Function GetAllAutoDownloadingRules() As String
                    Return Base.SendClientRequest(connectionPath & "rules")
                End Function

                Public Function getAllArticlesMatchingARule(RuleName As String) As String
                    Return Base.SendClientRequest(connectionPath & "matchingArticles?ruleName=" & RuleName)
                End Function

#End Region
            End Class

            Public Class SearchClass
                Friend Base As apiClass
                Private connectionPath As String = "/api/v2/search/"

#Region "Method"

                Public Function StartSearch(Pattern As String, Optional Plugins As String = "all", Optional Category As String = "all") As String
                    Return Base.SendClientRequest(connectionPath & "start?pattern=" & Pattern & "&plugins=" & Plugins & "&category=" & Category)
                End Function

                Public Function StopSearch(Id As String) As String
                    Return Base.SendClientRequest(connectionPath & "stop?id=" & Id)
                End Function

                Public Function GetSearchStatus(Optional Id As String = Nothing) As String
                    Dim StringBuilder As String = ""
                    If Id IsNot Nothing Then StringBuilder &= "?id=" & Id
                    Return Base.SendClientRequest(connectionPath & "status" & If(StringBuilder.Length > 0, StringBuilder, ""))
                End Function

                Public Function GetSearchResults(Id As String, Optional Limit As String = Nothing, Optional Offset As String = Nothing) As String
                    Dim StringBuilder As String = ""
                    If Limit IsNot Nothing Then StringBuilder &= "&limit=" & Limit
                    If Offset IsNot Nothing Then StringBuilder &= "&offset=" & Offset
                    Return Base.SendClientRequest(connectionPath & "results?id=" & Id & StringBuilder)
                End Function

                Public Function DeleteSearch(Id As String) As String
                    Return Base.SendClientRequest(connectionPath & "delete?id=" & Id)
                End Function

                Public Function GetSearchPlugins() As String
                    Return Base.SendClientRequest(connectionPath & "plugins")
                End Function

                Public Function InstallSearchPlugins(Sources As String) As String
                    Return Base.SendClientRequest(connectionPath & "installPlugin?sources=" & Sources)
                End Function

                Public Function UninstallSearchPlugins(Names As String) As String
                    Return Base.SendClientRequest(connectionPath & "uninstallPlugin?names=" & Names)
                End Function

                Public Function EnableSearchPlugins(Names As String, Enable As String) As String
                    Return Base.SendClientRequest(connectionPath & "enablePlugin?names=" & Names & "&enable=" & Enable)
                End Function

                Public Function UpdateSearchPlugins() As String
                    Return Base.SendClientRequest(connectionPath & "updatePlugins")
                End Function

#End Region
            End Class

            Public Sub New(this As apiClass)
                Base = this
                auth = New authClass
                auth.Base = Base
                app = New appClass
                app.Base = Base
                sync = New syncClass
                sync.Base = Base
                torrents = New torrentsClass
                torrents.Base = Base
                rss = New RSSClass
                rss.Base = Base
                search = New SearchClass
                search.Base = Base
            End Sub
        End Class

        Public Sub New()
            v2 = New v2Class(Me)
        End Sub

        Private Function SendClientRequest(URLString As String) As String
            Return v2.auth.SendClientRequest(URLString)
        End Function

        Public Function SendClientRequest(Path As String, ByRef Requests(,) As String) As String
            Return v2.auth.SendClientRequest(Path, Requests)
        End Function

        Public Function ParseJSON(Value As String) As Newtonsoft.Json.Linq.JObject
            Return v2.auth.ParseJSON(Value)
        End Function
    End Class

    Public Function GetWebUIVersion() As String
        Return WebUIVersion
    End Function

    Public Sub New()
        api = New apiClass()
    End Sub

End Class
