// Hàm lấy IP public của máy tính
async function getCurrentIP() {
    try {
        // Sử dụng ipify API để lấy IP public
        const response = await fetch('https://api.ipify.org?format=json');
        const data = await response.json();
        return data.ip;
    } catch (error) {
        console.error('Không thể lấy IP public:', error);
        return null;
    }
}

// Hàm lấy thông tin chi tiết về IP
async function getIPInfo() {
    try {
        const response = await fetch('https://ipapi.co/json/');
        const data = await response.json();
        return {
            ip: data.ip,
            country: data.country_name,
            city: data.city,
            region: data.region,
            timezone: data.timezone,
            isp: data.org
        };
    } catch (error) {
        console.error('Không thể lấy thông tin IP:', error);
        return null;
    }
}

// Hàm lấy IP local (nếu cần)
function getLocalIP() {
    return new Promise((resolve) => {
        const RTCPeerConnection = window.RTCPeerConnection || 
                                 window.webkitRTCPeerConnection || 
                                 window.mozRTCPeerConnection;
        
        if (!RTCPeerConnection) {
            resolve('127.0.0.1'); // Fallback
            return;
        }

        const pc = new RTCPeerConnection({ iceServers: [] });
        pc.createDataChannel('');
        pc.createOffer().then(offer => pc.setLocalDescription(offer));
        
        pc.onicecandidate = (event) => {
            if (!event.candidate) return;
            
            const ip = event.candidate.candidate.split(' ')[4];
            if (ip.indexOf('.') !== -1) {
                resolve(ip);
                pc.close();
            }
        };
        
        // Timeout sau 5 giây
        setTimeout(() => {
            resolve('127.0.0.1');
            pc.close();
        }, 5000);
    });
}

// Hàm gửi request với IP
async function sendRequestWithIP(url, data = {}) {
    try {
        const ip = await getCurrentIP();
        const ipInfo = await getIPInfo();
        
        const requestData = {
            ...data,
            clientIP: ip,
            ipInfo: ipInfo,
            timestamp: new Date().toISOString(),
            userAgent: navigator.userAgent,
            screenResolution: `${screen.width}x${screen.height}`,
            timezone: Intl.DateTimeFormat().resolvedOptions().timeZone
        };

        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Client-IP': ip,
                'X-User-Agent': navigator.userAgent
            },
            body: JSON.stringify(requestData)
        });

        return await response.json();
    } catch (error) {
        console.error('Lỗi khi gửi request:', error);
        throw error;
    }
} 