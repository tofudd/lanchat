//import
const colors = require("colors")
const client = require("./client")
const commands = require("./commands")
const settings = require("./settings")
const pkg = require("../package.json")
const rl = require("./interface").rl
const readline = require("./interface").readline

//PROMPT
module.exports = {
	run: function () {

		//init
		if (settings.load()) {
			process.stdout.write("\033c")
			process.stdout.write(
				String.fromCharCode(27) + "]0;" + "Lanchat" + String.fromCharCode(7)
			)
			console.log("Lanchat ".green + pkg.version.green)
			console.log("")
			rl.prompt(true)
		} else {
			console.log("Corrupted config file")
			process.exit(0)
		}

		//prompt
		rl.on("line", function (line) {
			wrapper(line)
			rl.prompt(true)
		})

		//exit
		rl.on("close", function () {
			process.stdout.write("\033c")
			process.exit(0)
		})
	}
}

//user input wrapper
function wrapper(message) {

	//check prefix
	if (message.charAt(0) !== "/") {

		//send message
		readline.moveCursor(process.stdout, 0, -1)
		client.send(message)

	} else {

		//execute command
		const args = message.split(" ")
		if (typeof commands[args[0].substr(1)] !== "undefined") {
			answer = commands[args[0].substr(1)](args.slice(1))
		}

		//reset cursor
		process.stdout.clearLine()
		process.stdout.cursorTo(0)
	}
}