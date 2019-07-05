Set objFso = CreateObject("Scripting.FileSystemObject")
Set Folder = objFSO.GetFolder("D:\Jacob_D\Documents\COMP 8045 Game 2 GitRepo\COMP 8045 Game 2")

For Each File In Folder.Files
    sNewFile = File.Name
    sNewFile = Replace(sNewFile,"SHMM","PM")
    if (sNewFile<>File.Name) then 
        File.Move(File.ParentFolder+"\"+sNewFile)
    end if

Next