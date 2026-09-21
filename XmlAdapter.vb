Public Class XmlAdapter
    Const DefaultID As Integer = 0

    Friend keyWordGroup As New Dictionary(Of String, List(Of Dictionary(Of Integer, List(Of String))))

    Public Sub New()
        keyWordGroup.Add("DEFAULT", New List(Of Dictionary(Of Integer, List(Of String))))
        keyWordGroup.Add("KEYWORD", New List(Of Dictionary(Of Integer, List(Of String))))
        keyWordGroup.Add("STRING", New List(Of Dictionary(Of Integer, List(Of String))))
        keyWordGroup.Add("COMMENT", New List(Of Dictionary(Of Integer, List(Of String))))
    End Sub

    Public Sub Load(path As String, ByRef scinilla As ScintillaNET.Scintilla)
        If Not IO.File.Exists(path) Then
            MsgBox(path & " 檔案不存在")
            Exit Sub
            'Throw New System.IO.FileNotFoundException()
        End If

        Dim xEle As XElement = XElement.Load(path)
        For Each style As XElement In xEle.<Style>
            Dim id As Integer = style.Attributes("ID")(0).Value
            Dim type As String = style.Attributes("Type")(0).Value

            For Each font As XElement In style.<Font>
                For Each attribs As XAttribute In font.Attributes()
                    Dim value As String = attribs.Value
                    Select Case attribs.Name.LocalName
                        Case "fontname"
                            scinilla.Styles(id).Font = attribs.Value
                        Case "Size"
                            scinilla.Styles(id).Size = attribs.Value
                        Case "SizeF"
                            scinilla.Styles(id).SizeF = attribs.Value
                        Case "ForeColor"
                            scinilla.Styles(id).ForeColor = Color.FromArgb(attribs.Value)
                        Case "BackColor"
                            scinilla.Styles(id).BackColor = Color.FromArgb(attribs.Value)
                        Case "Bold"
                            scinilla.Styles(id).Bold = IIf(attribs.Value = "true", True, False)
                        Case "Italic"
                            scinilla.Styles(id).Italic = IIf(attribs.Value = "true", True, False)
                        Case "Underline"
                            scinilla.Styles(id).Underline = IIf(attribs.Value = "true", True, False)
                        Case "Hotspot"
                            scinilla.Styles(id).Hotspot = IIf(attribs.Value = "true", True, False)
                        Case "FillLine"
                            scinilla.Styles(id).FillLine = IIf(attribs.Value = "true", True, False)
                        Case "Visible"
                            scinilla.Styles(id).Visible = IIf(attribs.Value = "true", True, False)
                    End Select
                Next

                If Not font.<KeyWord>.Count = 0 Then
                    Dim keyWords As New List(Of String)
                    For Each keyWord As XElement In font.<KeyWord>
                        keyWords.Add(keyWord.Value)
                    Next

                    Dim _keyWordID As New Dictionary(Of Integer, List(Of String))
                    _keyWordID.Add(id, keyWords)
                    Select Case type
                        Case "DEFAULT"
                            keyWordGroup("DEFAULT").Add(_keyWordID)
                        Case "KEYWORD"
                            keyWordGroup("KEYWORD").Add(_keyWordID)
                        Case "STRING"
                            keyWordGroup("STRING").Add(_keyWordID)
                        Case "COMMENT"
                            keyWordGroup("COMMENT").Add(_keyWordID)
                    End Select
                End If
            Next
        Next
    End Sub

    Public Sub StyleNeeded(ByRef scintilla As ScintillaNET.Scintilla)
        Dim startPos As Integer = scintilla.Lines(scintilla.FirstVisibleLine).Position  '取得顯示畫面最上行的位置
        Dim linesOnScreen As Integer = scintilla.LinesOnScreen                          '取得目前畫面能夠顯示的總行數
        Dim lastVisibleLineIndex As Integer = Math.Max(scintilla.Lines.Count - 1, scintilla.FirstVisibleLine + linesOnScreen - 1)   '確保不超過文件總行數
        Dim lastPos As Integer = scintilla.Lines(lastVisibleLineIndex).EndPosition      '取得畫面最下行的最後位置

        Dim text As String = scintilla.GetTextRange(startPos, lastPos - startPos)   '取得目前需要計算的文字區段

        scintilla.StartStyling(startPos)    '開始告訴 Scintilla 從哪裡開始上色

        Dim i As Integer = 0
        While i < text.Length
            If RuleA(i, text, scintilla) Then Continue While
            If RuleB(i, text, scintilla) Then Continue While
            If RuleC(i, text, scintilla) Then Continue While
            RuleD(i, text, scintilla)
        End While
    End Sub

    ''' <summary>
    ''' 規則A：開頭註解類
    ''' </summary>
    Function RuleA(ByRef i As Integer, ByRef text As String, ByRef scintilla As ScintillaNET.Scintilla) As Boolean
        Dim result As Boolean = False

        For Each group As Dictionary(Of Integer, List(Of String)) In keyWordGroup("COMMENT")
            For Each k As KeyValuePair(Of Integer, List(Of String)) In group
                For Each w As String In k.Value
                    Dim word As String = String.Empty
                    If i + w.Length <= text.Length Then word = text.Substring(i, w.Length)
                    If w = word Then
                        Dim commentLen As Integer = 0

                        '計算註解長度直到換行為止
                        While (i + commentLen) < text.Length AndAlso text(i + commentLen) <> ControlChars.Cr AndAlso text(i + commentLen) <> ControlChars.Lf
                            commentLen += 1
                        End While
                        scintilla.SetStyling(commentLen, k.Key)
                        i += commentLen
                        result = True
                    End If
                Next
            Next
        Next

        Return result
    End Function

    ''' <summary>
    ''' 規則B：前後包夾的字串類
    ''' </summary>
    Function RuleB(ByRef i As Integer, ByRef text As String, ByRef scintilla As ScintillaNET.Scintilla) As Boolean
        Dim result As Boolean = False

        For Each group As Dictionary(Of Integer, List(Of String)) In keyWordGroup("STRING")
            For Each k As KeyValuePair(Of Integer, List(Of String)) In group
                Dim front As String = k.Value(0)
                Dim back As String = k.Value(1)

                Dim frontWord As String = String.Empty
                If i + front.Length <= text.Length Then frontWord = text.Substring(i, front.Length)
                If front = frontWord Then
                    Dim comentLen As Integer = 1
                    While (i + comentLen) < text.Length
                        Dim backWord As String = String.Empty
                        If (i + comentLen + back.Length) <= text.Length Then backWord = text.Substring(i + comentLen, back.Length)
                        If back = backWord Then
                            result = True
                            comentLen += backWord.Length
                            Exit While
                        End If
                        comentLen += 1
                    End While

                    If result Then
                        scintilla.SetStyling(comentLen, k.Key)
                        i += comentLen
                    End If
                End If
            Next
        Next

        Return result
    End Function

    ''' <summary>
    ''' 規則C：關鍵字
    ''' </summary>
    Function RuleC(ByRef i As Integer, ByRef text As String, ByRef scintilla As ScintillaNET.Scintilla) As Boolean
        Dim result As Boolean = False
        Dim c As Char = text(i)

        If Char.IsLetter(c) Then
            Dim wordStart As Integer = i
            While i < text.Length AndAlso Char.IsLetterOrDigit(text(i))
                i += 1
            End While
            Dim word As String = text.Substring(wordStart, i - wordStart)

            '判斷是否為自訂關鍵字
            For Each group As Dictionary(Of Integer, List(Of String)) In keyWordGroup("KEYWORD")
                For Each k As KeyValuePair(Of Integer, List(Of String)) In group
                    For Each w As String In k.Value
                        If w = word Then
                            scintilla.SetStyling(word.Length, k.Key)
                            result = True
                            Exit For
                        End If
                    Next
                Next
            Next

            If Not result Then
                scintilla.SetStyling(word.Length, DefaultID)
            End If
        End If

        Return result
    End Function

    ''' <summary>
    ''' 規則D：預設
    ''' </summary>
    Sub RuleD(ByRef i As Integer, ByRef text As String, ByRef scintilla As ScintillaNET.Scintilla)
        If i < text.Length Then
            scintilla.SetStyling(1, DefaultID)
            i += 1
        End If
    End Sub
End Class