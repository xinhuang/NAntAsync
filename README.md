# NAntAsync

    <async target="long.run.target">
        <echo message="I'm doing something else while waiting..." />
        <echo message="waiting..." />
    </async>
    <echo message="Long run target finished!" />

    <async exec="copy_bulk_files.bat" failonerror="false">
        <echo message="I'm doing something else while waiting..." />
        <echo message="waiting..." />
    </async>
    <echo message="File copied!" />

NAntAsync is a NAnt custom task to execute NAnt target or external program asynchronously.

*Beware when assign NAnt property asynchronously. Unexpected race condition might happen since NAnt is not prepared for concurrent execution.*

## Feature

* `<async>` to execute target/external program
* Supported arguments as in <exec>, except for `managed`, `spawn`, `useruntimeengine`, `pidproperty`

## TODO

* Support for `<await>`

## License

* [GNU Lesser General Public License][1]


  [1]: http://www.gnu.org/copyleft/lgpl.html
