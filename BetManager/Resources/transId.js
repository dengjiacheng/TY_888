var tool={
    rotl: function (t, e) {
        return t << e | t >>> 32 - e
    },
    rotr: function (t, e) {
        return t << 32 - e | t >>> e
    },
    endian: function (t) {
        if (t.constructor == Number)
            return 16711935 & tool.rotl(t, 8) | 4278255360 & tool.rotl(t, 24);
        for (var e = 0; e < t.length; e++)
            t[e] = tool.endian(t[e]);
        return t
    },
    randomBytes: function (t) {
        for (var e = []; t > 0; t--)
            e.push(Math.floor(256 * Math.random()));
        return e
    },
    bytesToWords: function (t) {
        for (var e = [], n = 0, r = 0; n < t.length; n++,
            r += 8)
            e[r >>> 5] |= t[n] << 24 - r % 32;
        return e
    },
    wordsToBytes: function (t) {
        for (var e = [], n = 0; n < 32 * t.length; n += 8)
            e.push(t[n >>> 5] >>> 24 - n % 32 & 255);
        return e
    },
    bytesToHex: function (t) {
        for (var e = [], n = 0; n < t.length; n++)
            e.push((t[n] >>> 4).toString(16)),
                e.push((15 & t[n]).toString(16));
        return e.join("")
    },
    hexToBytes: function (t) {
        for (var e = [], n = 0; n < t.length; n += 2)
            e.push(parseInt(t.substr(n, 2), 16));
        return e
    },
    bytesToBase64: function (t) {
        for (var e = [], r = 0; r < t.length; r += 3)
            for (var o = t[r] << 16 | t[r + 1] << 8 | t[r + 2], i = 0; i < 4; i++)
                8 * r + 6 * i <= 8 * t.length ? e.push(n.charAt(o >>> 6 * (3 - i) & 63)) : e.push("=");
        return e.join("")
    },
    base64ToBytes: function (t) {
        t = t.replace(/[^A-Z0-9+\/]/gi, "");
        for (var e = [], r = 0, o = 0; r < t.length; o = ++r % 4)
            0 != o && e.push((my_export.strs.indexOf(t.charAt(r - 1)) & Math.pow(2, -2 * o + 8) - 1) << 2 * o | my_export.strs.indexOf(t.charAt(r)) >>> 6 - 2 * o);
        return e
    },
    stringToBytes_bin: function (t) {
        for (var e = [], n = 0; n < t.length; n++)
            e.push(255 & t.charCodeAt(n));
        return e
    },
    bytesToString_bin: function (t) {
        for (var e = [], n = 0; n < t.length; n++)
            e.push(String.fromCharCode(t[n]));
        return e.join("")
    },
    stringToBytes_utf_8: function (t) {
        return tool.stringToBytes_bin(unescape(encodeURIComponent(t)))
    },
    bytesToString_utf_8: function (t) {
        return decodeURIComponent(escape(tool.bytesToString_bin(t)))
    },
    unkonw_1: function (t) {
        return null != t && (!!t.constructor && "function" == typeof t.constructor.isBuffer && t.constructor.isBuffer(t) || function (t) {
            return "function" == typeof t.readFloatLE && "function" == typeof t.slice && n(t.slice(0, 0))
        }(t) || !!t._isBuffer)
    }

} 

function calculate(e, n) {
    e.constructor == String ? e = n && "binary" === n.encoding ? tool.stringToBytes_bin(e) : tool.stringToBytes_utf_8(e) : tool.unkonw_1(e) ? e = Array.prototype.slice.call(e, 0) : Array.isArray(e) || e.constructor === Uint8Array || (e = e.toString());
    for (var c = tool.bytesToWords(e), u = 8 * e.length, s = 1732584193, f = -271733879, l = -1732584194, d = 271733878, p = 0; p < c.length; p++)
        c[p] = 16711935 & (c[p] << 8 | c[p] >>> 24) | 4278255360 & (c[p] << 24 | c[p] >>> 8);
    c[u >>> 5] |= 128 << u % 32,
        c[14 + (u + 64 >>> 9 << 4)] = u;
    var h = function (t, e, n, r, o, i, a) {
        var c = t + (e & n | ~e & r) + (o >>> 0) + a;
        return (c << i | c >>> 32 - i) + e
    }
        , v = function (t, e, n, r, o, i, a) {
            var c = t + (e & r | n & ~r) + (o >>> 0) + a;
            return (c << i | c >>> 32 - i) + e
        }
        , m = function (t, e, n, r, o, i, a) {
            var c = t + (e ^ n ^ r) + (o >>> 0) + a;
            return (c << i | c >>> 32 - i) + e
        }
        , b = function (t, e, n, r, o, i, a) {
            var c = t + (n ^ (e | ~r)) + (o >>> 0) + a;
            return (c << i | c >>> 32 - i) + e
        };
    for (p = 0; p < c.length; p += 16) {
        var g = s
            , y = f
            , w = l
            , O = d;
        s = h(s, f, l, d, c[p + 0], 7, -680876936),
            d = h(d, s, f, l, c[p + 1], 12, -389564586),
            l = h(l, d, s, f, c[p + 2], 17, 606105819),
            f = h(f, l, d, s, c[p + 3], 22, -1044525330),
            s = h(s, f, l, d, c[p + 4], 7, -176418897),
            d = h(d, s, f, l, c[p + 5], 12, 1200080426),
            l = h(l, d, s, f, c[p + 6], 17, -1473231341),
            f = h(f, l, d, s, c[p + 7], 22, -45705983),
            s = h(s, f, l, d, c[p + 8], 7, 1770035416),
            d = h(d, s, f, l, c[p + 9], 12, -1958414417),
            l = h(l, d, s, f, c[p + 10], 17, -42063),
            f = h(f, l, d, s, c[p + 11], 22, -1990404162),
            s = h(s, f, l, d, c[p + 12], 7, 1804603682),
            d = h(d, s, f, l, c[p + 13], 12, -40341101),
            l = h(l, d, s, f, c[p + 14], 17, -1502002290),
            s = v(s, f = h(f, l, d, s, c[p + 15], 22, 1236535329), l, d, c[p + 1], 5, -165796510),
            d = v(d, s, f, l, c[p + 6], 9, -1069501632),
            l = v(l, d, s, f, c[p + 11], 14, 643717713),
            f = v(f, l, d, s, c[p + 0], 20, -373897302),
            s = v(s, f, l, d, c[p + 5], 5, -701558691),
            d = v(d, s, f, l, c[p + 10], 9, 38016083),
            l = v(l, d, s, f, c[p + 15], 14, -660478335),
            f = v(f, l, d, s, c[p + 4], 20, -405537848),
            s = v(s, f, l, d, c[p + 9], 5, 568446438),
            d = v(d, s, f, l, c[p + 14], 9, -1019803690),
            l = v(l, d, s, f, c[p + 3], 14, -187363961),
            f = v(f, l, d, s, c[p + 8], 20, 1163531501),
            s = v(s, f, l, d, c[p + 13], 5, -1444681467),
            d = v(d, s, f, l, c[p + 2], 9, -51403784),
            l = v(l, d, s, f, c[p + 7], 14, 1735328473),
            s = m(s, f = v(f, l, d, s, c[p + 12], 20, -1926607734), l, d, c[p + 5], 4, -378558),
            d = m(d, s, f, l, c[p + 8], 11, -2022574463),
            l = m(l, d, s, f, c[p + 11], 16, 1839030562),
            f = m(f, l, d, s, c[p + 14], 23, -35309556),
            s = m(s, f, l, d, c[p + 1], 4, -1530992060),
            d = m(d, s, f, l, c[p + 4], 11, 1272893353),
            l = m(l, d, s, f, c[p + 7], 16, -155497632),
            f = m(f, l, d, s, c[p + 10], 23, -1094730640),
            s = m(s, f, l, d, c[p + 13], 4, 681279174),
            d = m(d, s, f, l, c[p + 0], 11, -358537222),
            l = m(l, d, s, f, c[p + 3], 16, -722521979),
            f = m(f, l, d, s, c[p + 6], 23, 76029189),
            s = m(s, f, l, d, c[p + 9], 4, -640364487),
            d = m(d, s, f, l, c[p + 12], 11, -421815835),
            l = m(l, d, s, f, c[p + 15], 16, 530742520),
            s = b(s, f = m(f, l, d, s, c[p + 2], 23, -995338651), l, d, c[p + 0], 6, -198630844),
            d = b(d, s, f, l, c[p + 7], 10, 1126891415),
            l = b(l, d, s, f, c[p + 14], 15, -1416354905),
            f = b(f, l, d, s, c[p + 5], 21, -57434055),
            s = b(s, f, l, d, c[p + 12], 6, 1700485571),
            d = b(d, s, f, l, c[p + 3], 10, -1894986606),
            l = b(l, d, s, f, c[p + 10], 15, -1051523),
            f = b(f, l, d, s, c[p + 1], 21, -2054922799),
            s = b(s, f, l, d, c[p + 8], 6, 1873313359),
            d = b(d, s, f, l, c[p + 15], 10, -30611744),
            l = b(l, d, s, f, c[p + 6], 15, -1560198380),
            f = b(f, l, d, s, c[p + 13], 21, 1309151649),
            s = b(s, f, l, d, c[p + 4], 6, -145523070),
            d = b(d, s, f, l, c[p + 11], 10, -1120210379),
            l = b(l, d, s, f, c[p + 2], 15, 718787259),
            f = b(f, l, d, s, c[p + 9], 21, -343485551),
            s = s + g >>> 0,
            f = f + y >>> 0,
            l = l + w >>> 0,
            d = d + O >>> 0
    }
    return tool.endian([s, f, l, d])
}

function transId(t) {
    //t = "2|2856574|ah|h|0|eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJ7XCJ1c2VySWRcIjo4NTgxMDMsXCJhY2NvdW50XCI6XCJwZDJfMTg1MDc1NTgwOTNcIixcInBsYXRmb3JtXCI6XCJwZDJcIixcInR1cm5Qb2ludFwiOmZhbHNlLFwiZGV2aWNlVHlwZVwiOjB9IiwiY3JlYXRlZCI6MTY3ODk4MTI0OTExOSwiZXhwIjoxNjc5NTg2MDQ5fQ.yYK4UCsHR_ndH4QX5OT_aQxe8BKHhBWYe8H4ngbBWc9D9RU3tz-sWdAN8K4q1QJz7amuuvzhw8P8SVjhzsHtwA|1678982038699"
    var e = undefined;
    t = t + '|' + (new Date).getTime();
    var n = tool.wordsToBytes(calculate(t, e));
    return e && e.asBytes ? n : e && e.asString ? tool.bytesToString(n) : tool.bytesToHex(n)
}

// var p1 = '1|2837554|ah|h|0|eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJ7XCJ1c2VySWRcIjo4NTgxMDMsXCJhY2NvdW50XCI6XCJwZDJfMTg1MDc1NTgwOTNcIixcInBsYXRmb3JtXCI6XCJwZDJcIixcInR1cm5Qb2ludFwiOmZhbHNlLFwiZGV2aWNlVHlwZVwiOjB9IiwiY3JlYXRlZCI6MTY3ODQ0MzM2OTM1NCwiZXhwIjoxNjc5MDQ4MTY5fQ.Ysno5bElj2phSEVsKSdEkPtce1UDGxDhhjMQcVfLJi_wsIOU9jL0lsX1xQyYsjJ8xdHaUfL-N2ayZCHewYRH6Q|1678443379046'
// console.log(transId(p1))