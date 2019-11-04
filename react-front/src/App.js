import React from 'react';
import DataTable from './Resources/DataTable'

class App extends React.Component
{
  render()
  {
    return(
      <div>
        <div className="container-fluid">
          <div className="row py-2">
            <div className="col-md-6 d-flex justify-content-start">
              <a href="/#" className="navbar-brand pl-3">System <i className="fas fa-chart-bar"></i></a>
            </div>
          </div>
          <div className="row border">
            <div className="col">
              <ul className="nav justify-content-center ">
                <li className="nav-item">
                  <a className="nav-link active" href="/#"><i className="fas fa-briefcase"></i> Venta</a>
                </li>
                <li className="nav-item">
                  <a className="nav-link" href="/#"><i className="fas fa-people-carry"></i> Producción</a>
                </li>
                <li className="nav-item">
                  <a className="nav-link" href="/#"><i className="fa fa-users"></i> Clientes</a>
                </li>
                <li className="nav-item">
                  <a className="nav-link disabled" href="/#">Empleados</a>
                </li>
              </ul>
            </div>
          </div>
          <div className="row bg-light">
            <div className="col" id="body">
              
            <DataTable 
                keyHeaders={["idVenta","cliente","fecha", "fechaEntrega","total","boton"]}
                nameHeaders={["Id","Cliente","Fecha Carga", "Fecha Entrega", "Total vendido", "*"]}
                url={"https://localhost:5001/venta"}
                take={10}
                callBackData={(data)=>{
                  data.map((d, i) =>{
                    data[i].boton = <a href={"/#/venta/"+d.idVenta}>Ver</a>
                  })
                  return data;                
                }}
            />
            </div>
          </div>          
        </div>
      </div>
    )
  }  
}

export default App;

// export default ClickMe;

/*
  <DataTable 
                keyHeaders={["idVenta","nombre","fecha", "fechaEntrega","boton"]}
                nameHeaders={["Id","Nombre","Fecha Carga", "Fecha Entrega", "*"]}
                url={"https://localhost:5001/venta"}
                take={10}
                callBackData={(data)=>{
                  data.map((d, i) =>{
                    data[i].boton = <a href={"/#/venta/"+d.idVenta}>Ver</a>
                  })
                  return data;                
                }}
            />
*/