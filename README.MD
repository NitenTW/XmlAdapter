
# 簡介
XmlAdapter 是 ScintillaNet 懶得打程式碼，改用 XML 快速建立自訂高亮。

<br><br>

# 使用方法
宣告變數
```vbnet
Dim xmladapter1 As New XmlAdapter
```

<br>

並在 StyleNeeded() 事件中將宣告的 scinetilla1 傳入 xmladapter1 的參數
```vbnet
Private Sub scintilla1_StyleNeeded(sender As Object, e As StyleNeededEventArgs) Handles scintilla1.StyleNeeded
    xmladapter1.StyleNeeded(scintilla1)
End Sub
```

<br>

然後在表單的 Load() 事件裡讀入高亮的設定檔
```vbnet
Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    scintilla1.Dock = DockStyle.Fill
    scintilla1.WrapMode = ScintillaNET.WrapMode.Word    '換行

    Me.Controls.Add(scintilla1)

    scintilla1.StyleResetDefault()

    scintilla1.Technology = ScintillaNET.Technology.DirectWrite '開啟字型連字
    scintilla1.Styles(0).Font = "Fira Code" '0 為文件要使用的預設字型
    scintilla1.Styles(0).Size = 12
    scintilla1.StyleClearAll()

    '讀入xml中的各項設定 (必需在 scintilla1.StyleClearAll() 之後
    xmladapter1.Load("D:\sample.xml", scintilla1)
End Sub
```

<br><br>

# XML 自訂高亮
sample.xml
```xml
<?xml version="1.0" encoding="utf-8"?>
<Styles>
  <Style Type="DEFAULT" ID="0">
    <!--預設-->
    <Font Size="12" SizeF="12.0" />
  </Style>

  <Style Type="KEYWORD" ID="1">
    <!--關鍵字-->
    <Font fontname="Arial" Bold="true" Italic="true">
        <KeyWord>START</KeyWord>
    	<KeyWord>END</KeyWord>
    </Font>
  </Style>

  <Style Type="KEYWORD" ID="2">
    <!--關鍵字2-->
    <Font Color="-65536" Bold="true" Underline="true">
    	<KeyWord>LINE</KeyWord>
    </Font>
  </Style>

  <Style Type="STRING" ID="3">
    <!--字串-->
    <Font ForeColor="-16777216" BackColor="-1">
        <KeyWord>"</KeyWord>
    	<KeyWord>"</KeyWord>
    </Font>
  </Style>

  <Style Type="STRING" ID="4">
    <!--字串2-->
    <Font ForeColor="-16777216" BackColor="-1">
        <KeyWord>&lt;</KeyWord>
    	<KeyWord>/&gt;</KeyWord>
    </Font>
  </Style>

  <Style Type="COMMENT" ID="5">
    <!--註解-->
    <Font Hotspot="true" FillLine="true" Visible="true">
        <KeyWord>#</KeyWord>
    	<KeyWord>'</KeyWord>
    	<KeyWord>//</KeyWord>
    </Font>
  </Style>
</Styles>
```
分成四大類 **DEFAULT**、**KEYWORD**、**STRING**、**COMMENT**，將想高亮的關鍵字分別放入四類中。ID 0 是文件要用的預設字型，其他的只要 ID 不重覆即可。

ForeColor 的值用 Color.ToARGB() 的值

FillLine 是整行背景色

Hotspot 和 Visible 不知幹嘛用的，總之放上來

>ScintillaNet 會以最大字型為行距
