Function getHasPart(url) as Dynamic
	port = CreateObject("roMessagePort")
	obj = CreateObject("roUrlTransfer")
	obj.EnableFreshConnection(true)
	obj.SetPort(port)
	obj.SetUrl(url)
	return obj.GetToString()

End Function
Function getPage(requestUrl As String) as Void
	port = CreateObject("roMessagePort")
	obj = CreateObject("roUrlTransfer")
	obj.EnableFreshConnection(true)
	obj.SetPort(port)
	obj.SetUrl(requestUrl)
	resp = obj.GetToString()
End Function
Function getXML(requestUrl As String) as Dynamic
	port = CreateObject("roMessagePort")
	obj = CreateObject("roUrlTransfer")
	obj.EnableFreshConnection(true)
	obj.SetPort(port)
	obj.SetUrl(requestUrl)
	print "requesting:" + requestUrl
	obj.AddHeader("Content-Type", "application/x-www-form-urlencoded")
	resp = obj.GetToString()
	
	xml=CreateObject("roXMLElement")
	if not xml.Parse(resp) then
		print "Can't parse feed:" + resp
		return invalid
	endif
	
	categories = xml.GetChildElements()
	oarray = CreateObject("roArray", 100, true)
	for each e in categories 
		name = e.GetName()
		o = CreateObject("roAssociativeArray")
		if name = "folder" then
			o.TargetType = "folder"
			o.ContentType = "season"
			o.RequestSubUrl = obj.Unescape(e.gettext())
			'o.HDPosterUrl = obj.Unescape(e.gettext())
			o.Description = e@name
			o.Title = e@name
			o.ShortDescriptionLine1 = e@name
			o.ShortDescriptionLine2 = e@name
		elseif name = "file" then
			o.TargetType="file"
			o.ShortDescriptionLine1 = e@name
			o.ShortDescriptionLine2 = "Playable Video"
			
			o.StreamBitrates=[(e@bitrate).ToInt()]
			o.StreamUrls = [obj.Unescape(e.gettext())]
			o.StreamQualities = ["HD"]
			o.PauseUrl = obj.Unescape(e@pauseurl)
			o.StopUrl = obj.Unescape(e@stopurl)
			o.ResumeUrl = obj.Unescape(e@resumeurl)
			o.StreamFormat = e@format
			o.Title = e@name
		elseif name = "tv" then
			o.TargetType="tv"
			o.ShortDescriptionLine1 = e@name
			o.ShortDescriptionLine2 = "TV"
			o.StreamBitrates=[(e@bitrate).ToInt()]
			o.StreamUrls = [obj.Unescape(e.gettext())]
			o.StreamQualities = ["HD"]
			o.StreamFormat = e@format
			o.Title = e@name

		endif

		oarray.push(o)
	next
	return oarray
End Function