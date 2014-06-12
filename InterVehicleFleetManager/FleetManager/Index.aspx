<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="FleetManager.Index" async="true"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
          
    
<script type="text/javascript" src="http://ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=7.0">



</script>




<head runat="server">
    <link href="StyleSheet1.css" rel="stylesheet" type="text/css" />
    <title></title>
</head>
<body>
    <center>
    <form id="form1" runat="server">
        <center>
        <div class="title">
            <h1>
                <asp:Timer ID="Timer1" runat="server" Interval="6000" OnTick="Timer1_Tick">
                </asp:Timer>
                <asp:ScriptManager ID="ScriptManager1" runat="server">
                </asp:ScriptManager>
                InterVech Fleet Manager: <small>Fleet Management</small></h1>
        </div>
        <div class="applewrap">
        <div id="Coordinate" style="position:absolute; top: 174px; left: 277px;" class="col">
            <asp:UpdatePanel ID="upanel2" runat="server">
            <ContentTemplate>
                    <table>
                <tr>
                    <td colspan="2">Coordinates of Current location</td>
                </tr>
                <tr>
                    <td class="auto-style1">Latitude</td>
                    <td class="auto-style1">
                        <asp:Label ID="lblLat" runat="server" Text="0.000"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>Longitude</td>
                    <td>
                        <asp:Label ID="lblLon" runat="server" Text="0.000"></asp:Label>
                    </td>
                </tr>
                
                
            </table>

                </ContentTemplate>
                </asp:UpdatePanel> 
                

            
        </div>
        <br />
        <div id="Telemetry" style="position:absolute; top: 172px; left: 16px;" class="col">
            <table>
                <tr>
                    <td id="TelemetryTitle" colspan="2">Telemetry Data</td>
                </tr>
                <tr>
                    <td>Speed</td>
                    <td>
                        <asp:Label ID="lvlSpeed" runat="server" Text="0 MPH"></asp:Label></td>
                </tr>
                <tr>
                    <td>Engine Speed</td>
                    <td>
                        <asp:Label ID="lblEngineSpeed" runat="server" Text="0 RPM"></asp:Label></td>
                </tr>
                <tr>
                    <td>Coolant Temp</td>
                    <td>
                        <asp:Label ID="lblCoolantTemp" runat="server" Text="0"></asp:Label></td>
                </tr>
                <tr>
                    <td>Fuel Pressure</td>
                    <td>
                        <asp:Label ID="lblFuelPressure" runat="server" Text="0"></asp:Label></td>
                </tr>
                <tr>
                    <td>Throttle Position</td>
                    <td>
                        <asp:Label ID="lblThrottle" runat="server" Text="0"></asp:Label></td>
                </tr>
                <tr>
                    <td>Mass Air Flow </td>
                    <td>
                        <asp:Label ID="lblMAirFlow" runat="server" Text="0"></asp:Label></td>
                </tr>
                <tr>
                    <td>Air Intake Temp</td>
                    <td>
                        <asp:Label ID="lblAirIntake" runat="server" Text="0"></asp:Label></td>
                </tr>
                <tr>
                    <td>Fuel Level</td>
                    <td>
                        <asp:Label ID="lblFuelLevel" runat="server" Text="0"></asp:Label></td>
                </tr>
            </table>
        </div>
        <div id="DropDown" style="position:absolute; top: 120px; left: 16px; right: 556px;" class="col">
            <asp:Label runat="server" Text="Select Vehicle"></asp:Label>
                    <asp:DropDownList ID="cboDropDown" runat="server" AutoPostBack="True"></asp:DropDownList>
        </div>
            <div id="LiveStream" style="position:absolute; top: 174px; left: 1152px;" class="col">
                <asp:label runat="server" Text="Live Camera"></asp:label>
                <br />
                <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Image ID="Image1" runat="server" Height="191px" Width="151px">
                        </asp:Image>
                        <br />
                        <asp:Label ID="Label2" runat="server" Text="Select date"></asp:Label>
                        <asp:DropDownList ID="cboDropDown0" runat="server" AutoPostBack="True">
                        </asp:DropDownList>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
                    </Triggers>
                </asp:UpdatePanel>
                <br />
            </div>
        </div>
                    <div id="mapDiv" style="position:absolute; top: 105px; left: 540px; height: 393px; width: 606px;" class="col">
            <asp:label ID="Label1" runat="server" text="Map"></asp:label>
        </div>
            </center>
        </form>
   </center>
</body>
</html>
