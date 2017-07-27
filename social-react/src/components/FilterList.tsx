import * as React from 'react';
import { connect } from 'react-redux';
import { FilterListItem, actionCreators } from '../store/FilterList';
import { ApplicationState } from '../store';

// At runtime, Redux will merge together...
type FilterListProps =
    { items: FilterListItem[] }
    & typeof actionCreators;

class FilterList extends React.Component<FilterListProps, {}>
{
    componentWillMount() {
        this.props.requestFilterList();
    }

    render() {
        return <div>
            <div>Filters</div>
            <div>
                <ul>
                    {this.props.items.map((filter, index) => <li key={index}>{filter.name} - {filter.unReadNum}</li>)}
                </ul>
                <button onClick={this.props.requestFilterList}>reload</button>
            </div>
        </div>
    }
}

function mapStateToProps(state: ApplicationState) {
    return {
        items: state.filterList
    }
}

function mapDispatchToProps(dispatch: any) {
    return {
        requestFilterList: () => dispatch(actionCreators.requestFilterList())
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(FilterList);