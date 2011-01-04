Function preShowHomeScreen(breadA=invalid, breadB=invalid) As Object

    port=CreateObject("roMessagePort")
    screen = CreateObject("roPosterScreen")
    screen.SetMessagePort(port)
    if breadA<>invalid and breadB<>invalid then
        screen.SetBreadcrumbText(breadA, breadB)
    end if

    screen.SetListStyle("flat-category")
    screen.setAdDisplayMode("scale-to-fit")
    return screen

End Function

Function showHomeScreen(screen,url="" As String) As Integer
    if url = "" or url = Invalid Then
	url = getServerAddress()
	print "getServerAddress success, url=" + url
    End If
    if url = Invalid then
	print "invalid url, return"
	return -1
    End If
    objlist = getXML(url)
    if objlist = Invalid then
	if showSettingsScreen("Cannot Connect Remote Server, Please Verify/Change Server Settings") <> Invalid Then
		print "reshow home screen"
		showHomeScreen(screen,"")
	End If
	return -1
    else if url = getServerAddress()
        o = CreateObject("roAssociativeArray")
	o.TargetType="rokusettings"
	o.ShortDescriptionLine1 = "Settings"
	o.ShortDescriptionLine2 = "Roku Player Settings"
	objlist.push(o)
    end if
    screen.SetContentList(objlist)
    'screen.SetFocusedListItem(3)
    screen.Show()

    while true
        msg = wait(0, screen.GetMessagePort())
        if type(msg) = "roPosterScreenEvent" then
            print "showHomeScreen | msg = "; msg.GetMessage() " | index = "; msg.GetIndex()
            if msg.isListFocused() then
                print "list focused | index = "; msg.GetIndex(); " | category = "; m.curCategory
            else if msg.isListItemSelected() then
		print "list item selected | index = "; msg.GetIndex()
		if objlist[msg.GetIndex()].TargetType = "tv" then
		        objlist[msg.GetIndex()].StreamUrls[0] =  objlist[msg.GetIndex()].StreamUrls[0] + "/" + getChannelToWatch() + ".m3u8"
			showVideoScreen(objlist[msg.GetIndex()],url)
		else if objlist[msg.GetIndex()].TargetType = "file" then
			showVideoScreen(objlist[msg.GetIndex()],url)
			exit while
		else if objlist[msg.GetIndex()].TargetType = "folder" then
			newscreen = preShowHomeScreen("", "")
			showHomeScreen(newscreen, objlist[msg.GetIndex()].RequestSubUrl)
			exit while
		else if objlist[msg.GetIndex()].TargetType = "rokusettings" then
			if showSettingsScreen() then
				newscreen = preShowHomeScreen("", "")
				showHomeScreen(newscreen,getServerAddress())
				return 1
			End If
		end if
            else if msg.isScreenClosed() then
	        print "screen closed."
	        screenClosed(url)	        
                return -1
            end if
        end If
    end while

    return 0

End Function


Function showVideoScreen(episode As Object,fileUrl As String)
    port = CreateObject("roMessagePort")
    screen = CreateObject("roVideoScreen")
    screen.SetMessagePort(port)
    print "playing:" + episode.StreamUrls[0]
    screen.SetContent(episode)
    screen.Show()
    while true
        msg = wait(0, port)

        if type(msg) = "roVideoScreenEvent" then
            print "showHomeScreen | msg = "; msg.getMessage() " | index = "; msg.GetIndex()
            if msg.isScreenClosed()
                print "Screen closed"
		newscreen = preShowHomeScreen("", "")
		if episode.TargetType <> "tv" then
			getPage(episode.StopUrl)
		end if
		showHomeScreen(newscreen, fileUrl)
                exit while
	    elseif msg.isPaused()
		print "Screen Paused"
		if episode.TargetType <> "tv" then
			getPage(episode.PauseUrl)
		end if
	    elseif msg.isResumed()
		if episode.TargetType <> "tv" then
		        getPage(episode.ResumeUrl)
		end if
		print "Screen Resumed"
            elseif msg.isRequestFailed()
                print "Video request failure: "; msg.GetIndex(); " " msg.GetData() 
            elseif msg.isStatusMessage()
                print "Video status: "; msg.GetIndex(); " " msg.GetData() 
            elseif msg.isButtonPressed()
                print "Button pressed: "; msg.GetIndex(); " " msg.GetData()
            else
                print "Unexpected event type: "; msg.GetType()
            end if
        else
            print "Unexpected message class: "; type(msg)
        end if
    end while

End Function

Function screenClosed(url as String)
	slashcount = 0
	lastindex = 0
	slashindex = Instr(0,url,"/")
	while(slashindex <> 0)
		lastindex = slashindex
		slashcount = slashcount + 1
		slashindex = Instr(slashindex + 1,url,"/")
	end while
	print "while exited,slashcount=" + Str(slashcount) + "url=" + url
	if slashcount >= 3 then
	    newscreen = preShowHomeScreen("", "")
	    print "show new screen" + left(url,lastindex - 1)
	    showHomeScreen(newscreen, left(url,lastindex - 1))
	end if
End Function

Function showSettingsScreen(errorText="Please Set Server Address" As String) as Boolean
	screen = CreateObject("roKeyboardScreen")
	port = CreateObject("roMessagePort")
	screen.SetMessagePort(port)
	screen.SetTitle("Please Verify Your Settings")
	sec = CreateObject("roRegistrySection", "RokuLocalPlayer")
	if sec.Exists("ServerAddress")
		screen.SetText(sec.Read("ServerAddress"))
	End If
	screen.SetDisplayText(errorText)
	screen.AddButton(1, "OK")
	screen.AddButton(2, "Cancel")
	screen.Show()
	while true
		msg = wait(0, screen.GetMessagePort())
		print "message received"
		if type(msg) = "roKeyboardScreenEvent"
			if msg.isScreenClosed()
				print "showSettingsScreen Closed"
				return false
			else if msg.isButtonPressed() then
				'print "Evt: msg.GetMessage();" idx:"; msg.GetIndex()
				if msg.GetIndex() = 1
					serverAddress = screen.GetText()
					print "Set ServerAddress To Reg:" + serverAddress
					if serverAddress <> "" then
						sec = CreateObject("roRegistrySection", "RokuLocalPlayer")
						sec.Write("ServerAddress",serverAddress)
						sec.Flush()
						print "write ServerAddress To Reg success"
						return True
					End If
				else
					return false
				endif
			endif
		endif
	end while
End Function
Function getServerAddress() as Dynamic
	sec = CreateObject("roRegistrySection", "RokuLocalPlayer")
	if sec.Exists("ServerAddress")
		return sec.Read("ServerAddress")
	else
		if showSettingsScreen() then
			return sec.Read("ServerAddress")
		else 
			return Invalid
		end if
	end if
End Function

Function getChannelToWatch() As String
    port = CreateObject("roMessagePort")
    screen = CreateObject("roPinEntryDialog")

    screen.SetMessagePort(port)
    screen.SetTitle("Select The Channel You Want To Watch")
    screen.SetNumPinEntryFields(4)
    
    screen.AddButton(1, "OK")
    screen.AddButton(2, "CANCEL")
    screen.Show()

    while true
        msg = wait(0, screen.GetMessagePort())
        if type(msg) = "roPinEntryDialogEvent"
            if msg.isScreenClosed()
                return  ""          
            else if msg.isButtonPressed() then
                    print "Button pressed: "; msg.GetIndex(); " " msg.GetData()
                    if msg.GetIndex() = 1 then
		         screen.Close()
                         return screen.Pin()
		    else if msg.GetIndex() = 2 then
		    	 screen.Close()
			 return "CANCEL"
                    endif
            else
                print "Unknown event: "; msg.GetType(); " msg: "; msg.GetMessage()
            endif
        else 
            print "wrong type.... type=";msg.GetType(); " msg: "; msg.GetMessage()
        endif
    end while
    return ""
End Function