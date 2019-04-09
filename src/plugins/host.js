//import
const out = require("../lib/out")
const client = require("../lib/client")
const net = require("net")
const http = require("http").Server()
const io = require("socket.io")(http)
const { RateLimiterMemory } = require("rate-limiter-flexible")
var settings = require("../lib/settings")
var global = require("../lib/global")

//variavles
var motd

//HOST
module.exports = {

	//create host
	host: function () {

		//load db
		settings.loadDb()

		//check database
		var index = global.db.findIndex(x => x.nickname === global.nick)

		//create database index
		if (index === -1) {
			settings.addUser(global.nick)
		}

		//write permission to database
		settings.writedb(global.nick, "level", 5)

		//rate limiter init
		rateLimiter = new RateLimiterMemory(
			{
				points: global.ratelimit,
				duration: 1,
			})

		out.status("starting server")

		//load motd
		motd = global.motd
		if (!motd) {
			out.status("motd not found")
		}

		//check port
		testPort(global.port, "127.0.0.1", function (e) {
			if (e === "failure") {
				http.listen(global.port, function () {
					out.status("server running")
				})
				//start host
				run()
				global.server_status = true
				client.connect("localhost")
			} else {
				out.alert("Server is already running on this PC")
				return
			}
		})
	},
}

//ALL SERVER THINGS
function run() {
	io.on("connection", function (socket) {

		//BASIC ACTIONS
		//login
		socket.on("login", function (nick) {

			//detect blank nick
			if (!nick) {
				nick = "default"
			}

			//shortening long nick
			if (nick.length > 15) {
				nick = nick.substring(0, 15)
			}

			//check database
			var index = global.db.findIndex(x => x.nickname === nick)

			//create database index
			if (index === -1) {
				settings.addUser(nick)
			}

			//update index
			index = global.db.findIndex(x => x.nickname === nick)

			//add keys
			if (!global.db[index].hasOwnProperty("level")) {
				settings.writedb(nick, "level", 2)
			}
			if (!global.db[index].hasOwnProperty("ip")) {
				settings.writedb(nick, "ip", socket.handshake.address)
			}
			if (!global.db[index].hasOwnProperty("lock")) {
				settings.writedb(nick, "lock", false)
			}
			if (!global.db[index].hasOwnProperty("pass")) {
				settings.writedb(nick, "pass", false)
			}

			//ban check
			var ban

			//chekc via ip
			for (var i = 0; i < global.db.length; i++) {
				if (global.db[i].level === 0) {
					ban = true
				}
			}

			//check via nick
			if (global.db[index].level === 0) {
				ban = true
			}

			//handle banned user
			if (ban) {

				//return
				socket.emit("rcode", "005")
				socket.emit("return", "BANNED")
				io.sockets.connected[socket.id].disconnect()
			} else {

				//check lock
				if (global.db[index].lock) {

					//return
					socket.emit("rcode", "006")
					socket.emit("return", "Account locked\nLogin with /login <password>")
				} else {
					auth(nick, socket)
				}
			}
		})

		//logoff
		socket.on("disconnect", function () {

			//find user
			var index = global.users.findIndex(x => x.id === socket.id)

			//if user authorized
			if (index !== -1) {

				//emit status
				socket.broadcast.to("main").emit("status", {
					content: "left the chat",
					nick: global.users[index].nickname
				})

				//delete user from table
				global.users.splice(global.users.indexOf(index), 1)
			}
		})

		//auth
		socket.on("auth", function (nick, password) {
			var index = global.db.findIndex(x => x.nickname === nick)
			if (global.users.findIndex(x => x.id === socket.id) !== -1) {
				socket.emit("rcode", "007")
				socket.emit("return", "You are already logged")
			} else {
				if (password === global.db[index].pass) {
					auth(nick, socket)
				} else {
					socket.emit("rcode", "008")
					socket.emit("return", "Wrong password")
				}
			}
		})

		//register
		socket.on("register", function (nick, password) {
			var index = global.users.findIndex(x => x.nickname === nick)
			if (password) {
				if (index !== -1) {
					settings.writedb(nick, "lock", true)
					settings.writedb(nick, "pass", password)
					socket.emit("rcode", "009")
					socket.emit("return", "Done")
				}
			} else {
				socket.emit("rcode", "010")
				socket.emit("return", "Password cannot be blank")
			}
		})

		//USER ACTIONS
		//message
		socket.on("message", async (content) => {

			//find user
			var index = global.users.findIndex(x => x.id === socket.id)

			//if user loggined
			if (global.users[index]) {

				//get user from db
				var index2 = global.db.findIndex(x => x.nickname === global.users[index].nickname)

				//check mute
				if (global.db[index2].level === 1) {
					socket.emit("rcode", "011")
					socket.emit("return", "muted")
				} else {
					if (content) {
						//check lenght
						try {
							//flood block
							await rateLimiter.consume(socket.handshake.address)
							//validate message

							//block long messages
							if (content.length > 1500) {
								socket.emit("rcode", "012")
								socket.emit("return", "FLOOD BLOCKED")
							} else {
								//emit message
								socket.broadcast.to("main").emit("message", {
									nick: global.users[index].nickname,
									content: content
								})
							}

						} catch (rejRes) {

							//emit alert
							socket.emit("rcode", "004")
							socket.emit("return", "FLOOD BLOCKED")
						}
					} else {
						//emit blank message error
						socket.emit("rcode", "001")
					}
				}
			}
		})

		//mention
		socket.on("mention", function (nick) {
			//find user
			var selected = global.users.findIndex(x => x.nickname === nick)
			//find user
			var index = global.users.findIndex(x => x.id === socket.id)
			//if user exist
			if (global.users[selected]) {
				//send mention
				socket.to(`${global.users[selected].id}`).emit("mentioned", global.users[index].nickname)
			} else {
				//send return
				socket.emit("rcode", "002")
				socket.emit("return", "This user not exist")
			}
		})

		//change nick
		socket.on("nick", function (nick) {

			//find user
			var index = global.users.findIndex(x => x.id === socket.id)

			//shorten the long nick
			if (nick.length > 15) {
				nick = nick.substring(0, 15)
			}

			//check is nick already used
			if (global.db.findIndex(x => x.nickname === nick) !== -1) {
				socket.emit("rcode", "003")
				socket.emit("return", "nick already used on this server")
			} else {

				//get old nick
				var old = global.users[index].nickname
				//save new nick
				global.users[index].nickname = nick
				settings.writedb(old, "nickname", nick)
				//send return to user
				socket.broadcast.to("main").emit("return", old + " change nick to " + nick)
			}
		})

		//list
		socket.on("list", function () {

			//crate user list
			var list = []
			list[0] = "\nUser List:"

			//add users to table
			for (i = 1; i < global.users.length + 1; i++) {
				var a = global.users[i - 1]

				list[i] = a.nickname + " (" + a.status + ")"
			}

			//emit table
			socket.emit("return", list.join("\n"))

		})

		//change status
		socket.on("changeStatus", function (value) {
			if (value === "online" || value === "dnd" || value === "afk") {
				//find user
				var index = global.users.findIndex(x => x.id === socket.id)
				//change user status
				global.users[index].status = value
				//emit status
				socket.broadcast.to("main").emit("status", {
					content: "is " + value,
					nick: global.users[index].nickname
				})
			} else {
				socket.emit("rcode", "001")
			}
		})

		//MODERATION
		//kick
		socket.on("kick", function (arg) {
			var index = global.users.findIndex(x => x.id === socket.id)
			if (index !== -1) {
				var index2 = global.db.findIndex(x => x.nickname === global.users[index].nickname)
				if (global.db[index2].level < 3) {
					socket.emit("rcode", "013")
					socket.emit("return", "You not have permission")
				} else {
					//check user
					var index3 = global.users.findIndex(x => x.nickname === arg)
					if (!global.users[index3]) {
						socket.emit("rcode", "002")
						socket.emit("return", "This user not exist")
					} else {
						//kick user
						socket.emit("return", "Kicked " + global.users[index3].nickname)
						io.sockets.connected[global.users[index3].id].disconnect()
					}
				}
			}
		})

		//ban
		socket.on("ban", function (arg) {
			//find user
			var index = global.users.findIndex(x => x.id === socket.id)
			if (index !== -1) {
				//find user
				var index2 = global.db.findIndex(x => x.nickname === global.users[index].nickname)
				//find user
				var index3 = global.db.findIndex(x => x.nickname === arg)
				if ((global.db[index2].level < 3) || (global.db[index2].level < global.db[index3].level)) {
					socket.emit("rcode", "013")
					socket.emit("return", "You not have permission")
				} else {
					if (!global.db[index3]) {
						socket.emit("rcode", "002")
						socket.emit("return", "This user not exist")
					} else {
						//ban user
						socket.emit("return", "Banned " + global.users[index3].nickname)
						settings.writedb(arg, "level", 0)
						io.sockets.connected[global.users[index3].id].disconnect()
					}
				}
			}
		})

		//unban
		socket.on("unban", function (arg) {
			//find user
			var index = global.users.findIndex(x => x.id === socket.id)
			if (index !== -1) {
				//find user
				var index2 = global.db.findIndex(x => x.nickname === global.users[index].nickname)
				//find user
				var index3 = global.db.findIndex(x => x.nickname === arg)
				if (global.db[index2].level < 3) {
					socket.emit("rcode", "013")
					socket.emit("return", "You not have permission")
				} else {
					if (!global.db[index3]) {
						socket.emit("rcode", "002")
						socket.emit("return", "This user not exist")
					} else {
						//ban user
						socket.emit("return", "Unbanned " + global.db[index3].nickname)
						settings.writedb(arg, "level", 2)
					}
				}
			}
		})

		//mute
		socket.on("mute", function (arg) {
			//find user
			var index = global.users.findIndex(x => x.id === socket.id)
			if (index !== -1) {
				//find user
				var index2 = global.db.findIndex(x => x.nickname === global.users[index].nickname)
				//find user
				var index3 = global.db.findIndex(x => x.nickname === arg)
				if ((global.db[index2].level < 3) || (global.db[index2].level < global.db[index3].level)) {
					socket.emit("rcode", "013")
					socket.emit("return", "You not have permission")
				} else {
					if (!global.db[index3]) {
						socket.emit("rcode", "002")
						socket.emit("return", "This user not exist")
					} else {
						//mute user
						socket.emit("return", "Muted " + global.db[index3].nickname)
						settings.writedb(arg, "level", 1)
					}
				}
			}
		})

		//unmute
		socket.on("unmute", function (nick, arg) {
			//find user
			var index = global.users.findIndex(x => x.id === socket.id)
			if (index !== -1) {
				//find user
				var index2 = global.db.findIndex(x => x.nickname === global.users[index].nickname)
				//find user
				var index3 = global.db.findIndex(x => x.nickname === arg)
				if (global.db[index2].level < 3) {
					socket.emit("rcode", "013")
					socket.emit("return", "You not have permission")
				} else {
					if (!global.db[index3]) {
						socket.emit("rcode", "002")
						socket.emit("return", "This user not exist")
					} else {
						//mute user
						socket.emit("return", "Unmuted " + global.db[index3].nickname)
						settings.writedb(arg, "level", 2)
					}
				}
			}
		})

		//change permission level
		socket.on("level", function (nick, arg) {
			//find user
			var index = global.users.findIndex(x => x.nickname === socket.id)
			if (index !== -1) {
				//find user
				var index2 = global.db.findIndex(x => x.nickname === global.users[index].nickname)
				//find user
				var index3 = global.db.findIndex(x => x.nickname === arg[0])
				if ((global.db[index2].level < 4) || (global.db[index2].level < global.db[index3].level)) {
					socket.emit("rcode", "013")
					socket.emit("return", "You not have permission")
				} else {
					if (!global.db[index3]) {
						socket.emit("rcode", "002")
						socket.emit("return", "This user not exist")
					} else {
						if (arg[1] >= 0 && arg[1] <= 4) {
							//change permission
							socket.emit("return", "Updated permission for " + global.db[index3].nickname)
							settings.writedb(arg[0], "level", Number(arg[1]))
						} else {
							socket.emit("rcode", "014")
							socket.emit("return", "Bad permission ID")
						}
					}
				}
			}
		})
	})
}

//FUNCIONS
//Auth
function auth(nick, socket) {

	//check connected list
	if (global.users.findIndex(x => x.nickname === nick) !== -1) {

		socket.emit("rcode", "003")
		socket.emit("return", "User already connected\nChange nick and try again")
		io.sockets.connected[socket.id].disconnect()
	} else {

		//create user objcet
		var user = {
			id: socket.id,
			nickname: nick,
			status: "online",
			ip: socket.handshake.address
		}

		//add user to array
		global.users.push(user)

		//broadcast status
		socket.broadcast.to("main").emit("status", {
			content: "join the chat",
			nick: nick
		})

		//emit logged
		socket.emit("rcode", "015")

		//emit motd
		if (motd) {
			socket.emit("motd", motd)
		}

		//join
		socket.join("main")

		return
	}
}

//test port
function testPort(port, host, cb) {
	var client = net.createConnection(port, host).on("connect", function (e) {
		cb("success", e)
		client.destroy()
	}).on("error", function (e) {
		cb("failure", e)
	})
}