import React from 'react';

let searchTimeout= null;
class DataTable extends React.Component
{
    constructor(props)
    {
        super();
        this.state = 
        {
            keyHeaders: props.keyHeaders,//["id","nombre","fecha"],
            nameHeaders: props.nameHeaders,
            dataVisible: [],
            dataCount:3,
            pages:0,
            url: props.url,
            inPage:1,
            take: props.take,
            search: ""
        }
        this.contador = 0;

        this.getDataFromServer();
    }
    render()
    {
        return(
            <div>
                <div className="row mb-2 mt-2">
                    <div className="col d-flex justify-content-end" >
                        <input type="search" placeholder="buscar" className="form-control-sm form-control w-25" id="search" onInput={()=>
                            {
                                clearTimeout(searchTimeout)
                                searchTimeout = setTimeout(()=> {
                                    this.setState({search: document.getElementById("search").value})
                                    this.getDataFromServer();
                                },300)
                        }}
                        />
                    </div>
                </div>
                <table className="table bg-white border table-hover table-sm">
                    <thead className="">
                        <tr>
                            {
                                this.state.nameHeaders.map(n =>
                                    <th key={n}>{n}</th>
                                )
                            }
                        </tr>
                    </thead>
                    <tbody>
                    {this.state.dataVisible.map(datavisible =>
                        <tr key={datavisible.idVenta}>
                        {
                            this.state.keyHeaders.map(column => 
                                <td key={column}>{datavisible[column]}</td>
                            )
                        }
                        </tr>
                    )}
                    </tbody>
                </table>
                <div className="row">
                    <div className="col-6 d-flex justify-content-center">Viendo {this.state.dataVisible.length} de {this.state.dataCount}</div>
                    <div className="col-6 d-flex justify-content-center">
                        <button className="m-2 btn btn-secondary btn-sm" onClick={()=>{
                            this.setState({inPage: this.contador === 0 ? 0 : this.contador--})
                            this.getDataFromServer();
                        }
                        }><i className="fas fa-chevron-left"></i> Anterior</button>
                        Pág. {this.contador+1}
                        <button className="m-2 btn btn-secondary btn-sm" onClick={()=>{
                            this.setState({inPage: this.contador++})
                            this.getDataFromServer();
                        }
                        }>Siguiente <i className="fas fa-chevron-right"></i></button>
                    </div>
                </div>
            </div>
        )
    }
    getDataFromServer()
    {
        fetch(`${this.state.url}?skip=${this.contador*this.state.take}&take=${this.state.take}&search=${this.state.search}`)
        .then(data => data.json())
        .then(data => this.props.callBackData(data))
        .then(data => this.setState({dataVisible: data}))
    }
}

export default DataTable;