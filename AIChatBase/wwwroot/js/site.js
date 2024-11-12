//#region 加水印
//生成水印
function createWaterMarkerBase(str, width, height,) {
    let can = document.createElement('canvas');
    let body = document.body;
    body.appendChild(can);
    can.width = width;
    can.height = height;
    can.style.display = 'none';
    let cans = can.getContext('2d');
    cans.rotate(-30 * Math.PI / 180);
    if (isMobile()) {
        cans.font = "22px SimSun";
        cans.fillStyle = "rgba(17, 17, 17, 0.08)";
    } else {
        cans.font = "24px SimSun";
        cans.fillStyle = "rgba(17, 17, 17, 0.2)";
    }
    cans.textAlign = 'left';
    cans.fillText(str, -20, can.height);
    return can;
}

//判断是否手机
function isMobile() {
    return /Android|webOS|iPhone|iPod|BlackBerry/i.test(navigator.userAgent);
}

//获取缓存中的图片
function getWaterMarkerImg(userInfo) {
    let can = createWaterMarkerBase(userInfo, 240, 240);
    let url = can.toDataURL("image/png");
    return url
}

//添加水印
function addWaterMarker(markerimg) {
    var marker = document.getElementById(`water-marker`);
    if (marker) {
        marker.style.backgroundImage = `url('${markerimg}')`;
    }
}

//移除水印
function removeWaterMarker() {
    var marker = document.getElementById(`water-marker`);
    if (marker) {
        marker.style.backgroundImage = "";
    }
}
//#endregion


//移除水印
function isTopIframe() {
    if (window.self !== window.top) {
        return true;// 内嵌iframe
    }
    else
    {
        return false;
    }
}
//#endregion
