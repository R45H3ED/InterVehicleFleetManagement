<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="FleetManager.WebForm1" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="StyleSheet1.css" rel="stylesheet" type="text/css" />

    <title></title>
    <script>
        function a() {
            window.opener = null
            window.close();
        }
    </script>
</head>

<body>
    <center>
    <form id="form1" runat="server">
                <div class="title">
            <h1>InterVech Fleet Manager: <small>Login page</small></h1>
        </div>
    <div class="w">
        <div class="formwrap">
            <label for="Username">Username</label>
            <asp:TextBox ID="txtUname" runat="server" CssClass="shadowfield" placeholder="Username"></asp:TextBox>
        </div>

        <div class="formwrap">
            <label for="Password">Password</label>
            <asp:TextBox ID="txtPass" runat="server" CssClass="shadowfield" placeholder="Password" TextMode="Password"></asp:TextBox>
        </div>
        <div>
            <asp:Button ID="btnLogin" runat="server" CssClass="btn" Text="Login" OnClick="btnLogin_Click"  />
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            </div>
        <br />

    </div>
    </form>
    </center>
</body>
</html>
