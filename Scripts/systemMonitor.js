// systemMonitor.js
$(document).ready(function () {
    // Initial setup
    initializePage();

    // Event listeners
    $("#refreshButton").click(refreshSystemInfo);
    $("#runScanButton").click(runSystemScan);
    $("#assistantButton").click(toggleAssistant);
    $("#closeAssistant").click(closeAssistant);

    // Add event listeners for quick action buttons
    $(".fix-button").click(handleFixIssue);
    $("#checkUpdatesButton, #cleanupButton, #networkDiagnosticsButton, #startupAppsButton, #securityScanButton")
        .click(handleQuickAction);

    // Start refresh timer
    setInterval(refreshSystemInfo, 5000);

    // Hide assistant panel initially
    $("#assistantPanel").hide();
});

function initializePage() {
    // This would normally fetch real system data from the server
    // For demo purposes, we'll use simulated data
    refreshSystemInfo();
}

function refreshSystemInfo() {
    // In a real app, this would make an AJAX call to get real system info
    // For demo, we'll simulate with random data

    // CPU metrics
    const cpuUsage = Math.floor(Math.random() * 70) + 10;
    const cpuTemp = Math.floor(Math.random() * 40) + 35;
    $("#cpuUsageValue").text(cpuUsage + "%");
    $("#cpuTemperature").text(cpuTemp + "°C");

    // Update CPU health indicator
    if (cpuTemp > 80) {
        $("#cpuHealthIndicator").removeClass("healthy warning").addClass("danger");
    } else if (cpuTemp > 70) {
        $("#cpuHealthIndicator").removeClass("healthy danger").addClass("warning");
    } else {
        $("#cpuHealthIndicator").removeClass("warning danger").addClass("healthy");
    }

    // Memory metrics
    const memoryUsage = Math.floor(Math.random() * 50) + 20;
    const availableMemoryGB = Math.floor(Math.random() * 16) + 8;
    $("#memoryUsageValue").text(memoryUsage + "%");
    $("#memoryAvailable").text(availableMemoryGB + " GB Free");

    // Update Memory health indicator
    if (memoryUsage > 90) {
        $("#memoryHealthIndicator").removeClass("healthy warning").addClass("danger");
    } else if (memoryUsage > 80) {
        $("#memoryHealthIndicator").removeClass("healthy danger").addClass("warning");
    } else {
        $("#memoryHealthIndicator").removeClass("warning danger").addClass("healthy");
    }

    // Disk metrics
    const diskUsage = Math.floor(Math.random() * 30) + 60;
    const availableDiskGB = Math.floor(Math.random() * 200) + 50;
    $("#diskUsageValue").text(diskUsage + "%");
    $("#diskAvailable").text(availableDiskGB + " GB Free");

    // Update Disk health indicator
    if (diskUsage > 90) {
        $("#diskHealthIndicator").removeClass("healthy warning").addClass("danger");
    } else if (diskUsage > 75) {
        $("#diskHealthIndicator").removeClass("healthy danger").addClass("warning");
    } else {
        $("#diskHealthIndicator").removeClass("warning danger").addClass("healthy");
    }

    // Network metrics
    const networkSpeed = Math.floor(Math.random() * 49) + 1;
    const networkConnected = Math.random() < 0.9; // 90% chance of being connected
    $("#networkSpeedValue").text(networkSpeed + " MB/s");
    $("#networkStatus").text(networkConnected ? "Connected" : "Disconnected");

    // Update Network health indicator
    if (!networkConnected) {
        $("#networkHealthIndicator").removeClass("healthy warning").addClass("danger");
    } else if (networkSpeed < 5) {
        $("#networkHealthIndicator").removeClass("healthy danger").addClass("warning");
    } else {
        $("#networkHealthIndicator").removeClass("warning danger").addClass("healthy");
    }
}

function runSystemScan() {
    $("#runScanButton").text("Scanning...").attr("disabled", true);

    // Simulate scan progress
    showAssistantMessage("I'm running a full system scan. This might take a minute...");

    // After 3 seconds, complete the scan
    setTimeout(function () {
        refreshSystemInfo();
        $("#runScanButton").text("Run System Scan").attr("disabled", false);
        showAssistantMessage("System scan complete! Everything looks good, but you should free up some disk space soon.");
    }, 3000);
}

function handleQuickAction(e) {
    const action = $(this).attr("id").replace("Button", "");

    switch (action) {
        case "checkUpdates":
            showAssistantMessage("Checking for Windows updates. This might take a moment...");
            break;
        case "cleanup":
            showAssistantMessage("Disk Cleanup helps free up space by removing temporary files and emptying the Recycle Bin.");
            break;
        case "networkDiagnostics":
            showAssistantMessage("Running network diagnostics to check your internet connection...");
            break;
        case "startupApps":
            showAssistantMessage("Managing startup apps can help your computer boot faster. Disable programs you don't need to start automatically.");
            break;
        case "securityScan":
            showAssistantMessage("Running a quick security scan to check for threats...");
            break;
    }
}

function handleFixIssue() {
    const issueType = $(this).data("issue");

    switch (issueType) {
        case "DiskSpace":
            showAssistantMessage("I'm launching Disk Cleanup to help free up space. This will remove temporary files and empty your Recycle Bin.");
            break;
        case "WindowsUpdate":
            showAssistantMessage("Installing Windows updates helps keep your computer secure and running smoothly. After updates are installed, a restart will be needed.");
            break;
    }
}

function toggleAssistant() {
    if ($("#assistantPanel").is(":visible")) {
        closeAssistant();
    } else {
        openAssistant();
    }
}

function openAssistant() {
    $("#assistantPanel").fadeIn();
    showAssistantMessage("I'm here to help you understand your system better. What would you like to know?");
}

function closeAssistant() {
    $("#assistantPanel").fadeOut();
}

function showAssistantMessage(message) {
    // Fade out, change text, fade in
    $("#assistantMessage").fadeOut(150, function () {
        $(this).text(message).fadeIn(150);
    });

    // Make sure panel is visible
    if (!$("#assistantPanel").is(":visible")) {
        openAssistant();
    }
}