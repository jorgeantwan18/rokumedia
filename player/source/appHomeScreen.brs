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

Function showHomeScreen(screen,url="http://192.168.1.10:89" As String) As Integer
    objlist = getXML(url)
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
		else
			newscreen = preShowHomeScreen("", "")
			showHomeScreen(newscreen, objlist[msg.GetIndex()].RequestSubUrl)
			exit while
		end if
		
                
		return -1
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